# Hangfire - Background Jobs Guide

## 📋 O Que Foi Implementado

Uma feature completa de **Background Jobs** usando **Hangfire 1.8.14** com SQL Server como storage persistente.

### Arquitetura

```
Features/BackgroundJobs/
├── IBackgroundJob.cs                    # Interface base para todos os jobs
├── Commands/
│   ├── EnqueueJobCommand.cs            # Enfileira job para execução imediata
│   └── ScheduleJobCommand.cs           # Agenda job para tempo futuro
├── DTOs/
│   └── JobResultDto.cs                 # DTO com status do job
└── Jobs/
    ├── ProcessarNovoProdutoJob.cs      # Processa novo produto (simula email, etc)
    ├── SincronizarEstoqueJob.cs        # Sincroniza com sistema externo (recurring)
    └── LimpezaCacheJob.cs              # Limpa cache expirado (daily)

Controllers/
└── BackgroundJobsController.cs          # Endpoints para gerenciar jobs

Extensions/
└── HangfireExtensions.cs               # Setup de DI + Recurring Jobs
```

---

## 🚀 Endpoints

### Enfileirar Job (Executa ASAP)
```bash
POST /api/v1/backgroundjobs/enqueue?jobType=ProcessarNovoProduto

Response (202 Accepted):
{
  "data": {
    "jobId": "1",
    "jobType": "ProcessarNovoProduto",
    "status": "Enfileirado",
    "enqueuedAt": "2026-04-30T13:45:23Z"
  },
  "mensagem": "Job enfileirado com sucesso..."
}
```

### Agendar Job (Executa em tempo futuro)
```bash
POST /api/v1/backgroundjobs/schedule?jobType=ProcessarNovoProduto&delaySeconds=30

Response (202 Accepted):
{
  "data": {
    "jobId": "2",
    "jobType": "ProcessarNovoProduto",
    "status": "Agendado",
    "enqueuedAt": "2026-04-30T13:45:23Z",
    "scheduledFor": "2026-04-30T13:46:00Z"
  }
}
```

### Dashboard Info
```bash
GET /api/v1/backgroundjobs/dashboard-info

Response: Informações sobre jobs disponíveis e exemplos de uso
```

### Hangfire Dashboard
Acesse: **http://localhost:5000/hangfire**

Dashboard visual com:
- Jobs em fila
- Jobs em processamento
- Jobs completados
- Jobs falhados
- Recurring jobs agendados
- Logs de execução

---

## 🎯 Jobs Disponíveis

### 1. ProcessarNovoProdutoJob
**Tipo**: On-demand
**Descrição**: Simula processamento de novo produto (email, indexação, etc)
**Enqueue**:
```bash
POST /api/v1/backgroundjobs/enqueue?jobType=ProcessarNovoProduto
```

### 2. SincronizarEstoqueJob
**Tipo**: Recurring (a cada 5 minutos)
**Descrição**: Sincroniza estoque com ERP/sistema externo
**CRON**: `*/5 * * * *` (a cada 5 minutos)
**Manual Enqueue**:
```bash
POST /api/v1/backgroundjobs/enqueue?jobType=SincronizarEstoque
```

### 3. LimpezaCacheJob
**Tipo**: Recurring (diariamente às 03:00 UTC)
**Descrição**: Remove entradas antigas de cache
**CRON**: `0 3 * * *` (diariamente às 03:00)
**Manual Enqueue**:
```bash
POST /api/v1/backgroundjobs/enqueue?jobType=LimpezaCache
```

---

## 🔧 Configuração

### appsettings.json
```json
{
  "Hangfire": {
    "WorkerCount": 4,
    "DashboardUrl": "/hangfire",
    "Description": "Hangfire uses the 'SqlServer' connection string for job storage"
  }
}
```

Hangfire usa a mesma `ConnectionString:SqlServer` para persistência.

### Program.cs
```csharp
// Services registration
builder.Services.AddHangfireServices(builder.Configuration);

// Pipeline configuration
app.UseHangfireConfiguration();  // Dashboard + Recurring Jobs
```

### Aplicação.csproj
```xml
<ItemGroup>
  <PackageReference Include="Hangfire.Core" Version="1.8.14" />
  <PackageReference Include="Hangfire.SqlServer" Version="1.8.14" />
  <PackageReference Include="Hangfire.AspNetCore" Version="1.8.14" />
</ItemGroup>
```

---

## 💡 Como Funciona

### Fluxo de Execução

1. **Client** → Envia request para `/api/v1/backgroundjobs/enqueue`
2. **Controller** → Cria Command (EnqueueJobCommand)
3. **MediatR** → Roteia para Handler
4. **Handler** → Enfileira no Hangfire via `IBackgroundJobClient.Enqueue<T>()`
5. **Hangfire Server** → Persiste job em SQL Server
6. **Worker Thread** → Executa job quando disponível
7. **Job** → Executa lógica assíncrona
8. **Dashboard** → Mostra status em tempo real

### Persistência

- **Storage**: SQL Server (mesma DB da aplicação)
- **Tabelas Criadas Automaticamente**: `HangfireCounter`, `HangfireHash`, `HangfireJob`, `HangfireList`, `HangfireSet`, `HangfireState`
- **Worker Count**: 2 × CPU cores (ajustável em HangfireExtensions.cs)
- **Queues**: `default`, `critical`, `background`

---

## 📊 Exemplo de Uso Completo

### Criar Job Recorrente Novo

**Arquivo**: `Application/Features/BackgroundJobs/Jobs/NotificarEstoqueJob.cs`

```csharp
public class NotificarEstoqueJob : IBackgroundJob
{
    private readonly IProdutoRepository _repo;
    private readonly ILogger<NotificarEstoqueJob> _logger;

    public NotificarEstoqueJob(IProdutoRepository repo, ILogger<NotificarEstoqueJob> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Verificando produtos com estoque baixo...");
        
        var produtos = await _repo.GetAllAsync();
        var baixoEstoque = produtos.Where(p => p.QuantidadeEstoque < 5);

        foreach (var p in baixoEstoque)
        {
            _logger.LogWarning("Estoque baixo: {Produto} ({Qtd})", p.Nome, p.QuantidadeEstoque);
            // Enviar notificação, email, etc
        }
    }
}
```

**Registrar**:
1. Adicionar em `EnqueueJobCommand`:
   ```csharp
   "NotificarEstoque" => _hangfireClient.Enqueue<NotificarEstoqueJob>(...)
   ```

2. Adicionar recurring job em `HangfireExtensions.cs`:
   ```csharp
   recurringJobManager.AddOrUpdate<NotificarEstoqueJob>(
       "notificar-estoque-baixo",
       job => job.ExecuteAsync(CancellationToken.None),
       "0 9 * * *"  // Diariamente às 09:00
   );
   ```

3. Registrar no DI:
   ```csharp
   services.AddTransient<NotificarEstoqueJob>();
   ```

---

## 🔍 Monitorando Jobs

### Via Dashboard
- Acesse: http://localhost:5000/hangfire
- Visualize: Enfileirados, Processando, Completados, Falhados
- Actions: Retry, Delete, Requeue jobs

### Via Logs
- Serilog captura tudo (Console + Seq)
- Procure por "Executando", "Completado", "Erro" no Seq
- Seq: http://localhost:5341

### Exemplo de Log
```
[13:45:23 INF] 📋 Enfileirando job: ProcessarNovoProduto
[13:45:24 INF] 🔄 Iniciando ProcessarNovoProdutoJob...
[13:45:26 INF] ✅ ProcessarNovoProdutoJob completado com sucesso!
```

---

## ⚠️ Tratamento de Erros

Jobs que lançam exceção são **automaticamente retentados** pelo Hangfire.

### Retry Policy (Padrão)
- Tentativa 1: Imediata
- Tentativa 2: +10 segundos
- Tentativa 3: +10 segundos
- Tentativa 4+: +30 segundos

### Customizar Retry
```csharp
public class MyJob : IBackgroundJob
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Lógica
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job falhou - será retentado");
            throw;  // Hang fire irá retentar
        }
    }
}
```

---

## 📈 Performance & Scaling

### Tuning
- **WorkerCount**: Alterar em `HangfireExtensions.cs`
  ```csharp
  options.WorkerCount = 8;  // Default: CPU cores * 2
  ```

- **Queue Polling**: `QueuePollInterval = TimeSpan.FromSeconds(15)`

- **Batch Size**: `CommandBatchMaxTimeout = TimeSpan.FromMinutes(5)`

### Load Balancing
Hangfire suporta múltiplas instâncias da aplicação compartilhando o mesmo SQL Server:
- Instância 1 → Worker 1, 2, 3, 4
- Instância 2 → Worker 5, 6, 7, 8
- SQL Server → Job queue centralizado

---

## 🧪 Testing

### Unit Test de Job
```csharp
[Fact]
public async Task ProcessarNovoProdutoJob_ShouldComplete()
{
    var mockLogger = new Mock<ILogger<ProcessarNovoProdutoJob>>();
    var job = new ProcessarNovoProdutoJob(mockLogger.Object);

    await job.ExecuteAsync();

    mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("completado")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()
        ),
        Times.Once
    );
}
```

### Integration Test
```csharp
[Fact]
public async Task EnqueueJob_ShouldReturnJobId()
{
    var command = new EnqueueJobCommand("ProcessarNovoProduto");
    var handler = new EnqueueJobCommandHandler(_hangfireClient, _logger);

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.NotEmpty(result.JobId);
    Assert.Equal("ProcessarNovoProduto", result.JobType);
}
```

---

## 🛡️ Segurança

- ✅ Endpoints protegidos com `[Authorize]`
- ✅ Jobs persistem em banco privado
- ✅ Dashboard readOnly por padrão (editar em `HangfireExtensions.cs`)
- ✅ Sem exposição de connection string no dashboard
- ✅ Logs estruturados (Serilog)

---

## 📚 Recursos Adicionais

- **Hangfire Docs**: https://docs.hangfire.io/
- **CRON Expressions**: https://crontab.guru/
- **SQL Server Storage**: https://docs.hangfire.io/en/latest/configuration/using-sql-server.html

---

## 📝 Próximos Passos

1. **Criar novo Job**:
   - Implementar `IBackgroundJob`
   - Registrar em DI
   - Adicionar em Command handlers
   - Testar via endpoint

2. **Adicionar Recurring Job**:
   - Implementar job
   - Adicionar em `HangfireExtensions.cs` com CRON
   - Verificar no dashboard

3. **Monitoramento em Produção**:
   - Alertas para jobs que falham repetidamente
   - Dashboard acessível apenas com autenticação
   - Logs centralizados (ELK, Splunk, etc)

---

*Implementado em 30/04/2026*
