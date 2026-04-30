# TL.Exemplo - Vertical Slice API

## 📋 Visão Geral

Uma API ASP.NET Core 8 que demonstra a arquitetura **Vertical Slice** com foco em separação por features, aplicando CQRS (Command Query Responsibility Segregation) através do **MediatR**, validação de negócio com **FluentValidation**, persistência com **Dapper**, cache com **Redis**, observabilidade com **Serilog** e **OpenTelemetry**, e suporte para mensageria com **RabbitMQ** e **Kafka**.

---

## 🏗️ Arquitetura - Vertical Slice

A arquitetura é organizada por **features verticais** ao invés de camadas horizontais. Cada slice (Produtos, Authentication, Cache, etc) contém:
- **Command/Query**: Contrato de entrada (CQRS)
- **Validator**: Regras de validação com FluentValidation
- **Handler**: Lógica de negócio
- **Controller**: Endpoint HTTP

Exemplo de slice de Produtos:
```
Features/Produtos/
├── Commands/
│   ├── CreateProduto/       → CreateProdutoCommand + Validator + Handler
│   ├── UpdateProduto/       → UpdateProdutoCommand + Validator + Handler
│   └── DeleteProduto/       → DeleteProdutoCommand + Validator + Handler
├── Queries/
│   ├── GetAllProdutos/      → GetAllProdutosQuery + Handler
│   ├── GetProdutoById/      → GetProdutoByIdQuery + Handler
│   └── GetProdutosPaged/    → GetProdutosPagedQuery + Handler
└── ProdutosController.cs    → Endpoints HTTP
```

Cada feature é independente e pode evoluir/ser testada isoladamente.

---

## 📦 Stack Tecnológico

| Componente | Versão | Propósito |
|-----------|--------|----------|
| **.NET** | 8.0 | Framework |
| **MediatR** | 12.3.0 | CQRS e mediação de commands/queries |
| **FluentValidation** | 11.9.2 | Validação declarativa |
| **Dapper** | (via Repositories) | ORM lightweight para SQL |
| **SQL Server** | 2022 | Banco de dados relacional |
| **Redis** | Latest | Cache distribuído |
| **RabbitMQ** | 3 Management | Fila de mensagens (pub/sub) |
| **Kafka** | Latest | Streaming de eventos |
| **Serilog** | 8.0.0 | Logging estruturado |
| **Seq** | 2024.3 | UI para visualizar logs |
| **OpenTelemetry** | 1.9.0 | Distributed tracing |
| **Jaeger** | Latest | Visualization de traces |
| **Swagger/Swashbuckle** | 6.9.0 | Documentação API |
| **JWT Bearer** | 8.0.0 | Autenticação |

---

## 📁 Estrutura de Pastas

```
TL.Exemplo-VerticalSlice/
├── src/
│   ├── TL.Exemplo.API/                 # Camada de apresentação (Controllers, Middleware)
│   │   ├── Features/                   # Slices verticais com Controllers
│   │   │   ├── Produtos/              # Feature de gerenciamento de produtos
│   │   │   ├── Authentication/        # Feature de autenticação
│   │   │   ├── Kafka/                 # Feature de produção/consumo Kafka
│   │   │   ├── Rabbit/                # Feature de produção/consumo RabbitMQ
│   │   │   └── Redis/                 # Feature de cache
│   │   ├── Extensions/                # DI containers (ServiceCollectionExtensions, HealthCheckExtensions)
│   │   ├── Middleware/                # Middlewares customizados (RateLimitingMiddleware, ExceptionHandling)
│   │   ├── Program.cs                 # Configuração da aplicação (Serilog, JWT, OTel, Services)
│   │   └── appsettings.json           # Configurações (Connection strings, JWT, Kafka, RabbitMQ)
│   │
│   ├── TL.Exemplo.Application/        # Camada de aplicação (Commands, Queries, Behaviors)
│   │   ├── Features/                  # Implementação de Commands/Queries por feature
│   │   │   ├── Produtos/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateProduto/
│   │   │   │   │   ├── UpdateProduto/
│   │   │   │   │   └── DeleteProduto/
│   │   │   │   └── Queries/
│   │   │   │       ├── GetAllProdutos/
│   │   │   │       ├── GetProdutoById/
│   │   │   │       └── GetProdutosPaged/
│   │   │   ├── Authentication/
│   │   │   ├── Cache/
│   │   │   ├── Kafka/
│   │   │   └── Rabbit/
│   │   ├── Common/
│   │   │   ├── Behaviors/             # Pipeline behaviors (ValidationBehavior, etc)
│   │   │   └── Models/                # DTOs e modelos comuns (ApiResponse, PaginationModels, etc)
│   │   └── Contracts/                 # Interfaces de dependências (IRepository, ICache, IMessaging)
│   │
│   ├── TL.Exemplo.Domain/             # Camada de domínio (Entities)
│   │   └── Entities/
│   │       └── Produto.cs             # Entidade de domínio
│   │
│   ├── TL.Exemplo.Infrastructure/     # Camada de infraestrutura (Implementações)
│   │   ├── Data/                      # Acesso a dados (DbConnectionFactory, Dapper)
│   │   ├── Repositories/              # Implementação de repositórios (ProdutoRepository)
│   │   ├── Cache/                     # Implementação de cache (RedisCacheService)
│   │   ├── Messaging/                 # Implementação de fila (RabbitMqService, KafkaService)
│   │   └── Authentication/            # Implementação de auth (JwtTokenService, UserRepository)
│   │
│   └── TL.Exemplo.Tests/              # Testes unitários e integração
│
├── database/                          # Scripts SQL para database
├── VerticalSlice/                    # Documentação e exemplos de patterns
├── docker-compose.yml                 # Infraestrutura local (na pasta pai)
└── PROJECT_INFO.md                   # Este arquivo

```

---

## 🔧 Configuração e Startup

### appsettings.json
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=.;Database=TLExemplo;Trusted_Connection=true;",
    "Redis": "localhost:6379"
  },
  "JwtSettings": {
    "SecretKey": "sua-chave-secreta-super-segura-com-mais-de-32-caracteres-aqui!!!",
    "Issuer": "TL.Exemplo.API",
    "Audience": "TL.Exemplo.Users",
    "ExpirationMinutes": 60
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  },
  "RabbitMq": {
    "HostName": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  }
}
```

### Startup em Program.cs
1. **Serilog**: Configurado ANTES do builder para capturar logs iniciais
2. **JWT**: Configuração de Bearer token + validação
3. **MediatR**: Registra todos os handlers da Application
4. **FluentValidation**: Registra validators + Pipeline behavior de validação
5. **Health Checks**: Endpoints `/health`, `/health/live`, `/health/ready`
6. **OpenTelemetry**: Instrumentação OTLP para Jaeger (localhost:4317)
7. **Middleware Pipeline**: Serilog Request Logging → Rate Limiting → Exception Handling

---

## 🔐 Autenticação e Autorização

- **Tipo**: JWT Bearer Token
- **Implementação**: `JwtTokenService` (Infrastructure)
- **Expiração**: 60 minutos (configurável)
- **Validação**: 
  - IssuerSigningKey ✓
  - Issuer ✓
  - Audience ✓
  - Lifetime ✓ (sem clock skew)
- **Headers Swagger**: Security definition para testar com JWT direto no Swagger

### Login
- **POST** `/api/v1/auth/login`
- Request: `{ "username": "string", "password": "string" }`
- Response: `{ "token": "jwt_token_here", "expiresIn": 3600 }`

### Controllers Protegidos
- Todos os endpoints de Produtos requerem `[Authorize]`
- Pass token no header: `Authorization: Bearer <jwt_token>`

---

## 📊 Persistência de Dados

### SQL Server + Dapper
- **DbConnectionFactory**: Factory pattern para criar conexões SQL
- **ProdutoRepository**: Implementação com Dapper para queries SQL puras
- Pattern: Async/await com `QueryAsync`, `QueryFirstOrDefaultAsync`, `ExecuteAsync`

### Produto Entity
```csharp
public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}
```

### DTOs
- **ProdutoDto**: DTO para transferência de dados (fields públicas readonly)
- **PagedResult<T>**: Wrapper para paginação (Items, PageNumber, PageSize, TotalCount)

---

## 💾 Cache (Redis)

- **Serviço**: `RedisCacheService` (Infrastructure)
- **Contrato**: `ICacheService`
- **Implementação**: Stack Exchange Redis
- **Métodos**: `GetAsync<T>`, `SetAsync<T>`, `RemoveAsync`
- **Prefix**: `TLExemplo_` (separação de instâncias)

### Exemplo de uso em Handlers
```csharp
// Guardar em cache
await _cacheService.SetAsync("produto:1", produto, TimeSpan.FromHours(1));

// Recuperar do cache
var produto = await _cacheService.GetAsync<ProdutoDto>("produto:1");
```

---

## 🔄 Background Jobs (Hangfire)

Processamento assíncrono de tarefas com **Hangfire 1.8.14** e SQL Server.

### Recursos
- **Enfileiramento**: Enfileire jobs para execução imediata
- **Agendamento**: Agende jobs para tempo futuro (delay em segundos)
- **Recurring Jobs**: Jobs automáticos em intervalos regulares (CRON)
- **Persistência**: SQL Server (mesmo banco da aplicação)
- **Dashboard Visual**: http://localhost:5000/hangfire
- **Retry Automático**: Falhas são automaticamente retentadas
- **Logging**: Integrado com Serilog para auditoria completa

### Jobs Pré-configurados
1. **ProcessarNovoProdutoJob**: On-demand, simula processamento (email, indexação)
2. **SincronizarEstoqueJob**: Recurring a cada 5 minutos, sincroniza com ERP
3. **LimpezaCacheJob**: Recurring diariamente às 03:00, limpa cache expirado

### Endpoints
- `POST /api/v1/backgroundjobs/enqueue?jobType=ProcessarNovoProduto` → Enfileira job (202)
- `POST /api/v1/backgroundjobs/schedule?jobType=ProcessarNovoProduto&delaySeconds=30` → Agenda job (202)
- `GET /api/v1/backgroundjobs/dashboard-info` → Info sobre jobs disponíveis
- `GET /hangfire` → Dashboard visual (não requer JWT)

### Exemplo de Uso
```bash
# Enfileirar job para ASAP
curl -X POST "http://localhost:5000/api/v1/backgroundjobs/enqueue?jobType=SincronizarEstoque" \
  -H "Authorization: Bearer <token>"

# Agendar job para 60 segundos no futuro
curl -X POST "http://localhost:5000/api/v1/backgroundjobs/schedule?jobType=ProcessarNovoProduto&delaySeconds=60" \
  -H "Authorization: Bearer <token>"
```

### Como Criar Novo Job
1. Implementar `IBackgroundJob` em `Application/Features/BackgroundJobs/Jobs/`
2. Registrar no DI em `HangfireExtensions.cs`
3. Adicionar case em `EnqueueJobCommand.cs` e `ScheduleJobCommand.cs`
4. (Opcional) Adicionar como recurring job em `HangfireExtensions.cs` com expressão CRON

Veja `VerticalSlice/HANGFIRE_GUIDE.md` para documentação completa.

---

## 📨 Mensageria

### RabbitMQ
- **Serviço**: `RabbitMqService` (Infrastructure)
- **Contrato**: `IRabbitMqService`
- **Fila Padrão**: `TL.Exemplo.Fila`
- **Métodos**: `EnviarMensagemAsync`, `ConsumirMensagensAsync`
- **Features**:
  - Pub/Sub de mensagens
  - Acknowledgment automático
  - Serialização JSON
  - Handler para consumo contínuo

### Kafka
- **Serviço**: `KafkaService` (Infrastructure)
- **Contrato**: `IKafkaService`
- **Tópico Padrão**: `TL.Exemplo.Eventos`
- **Métodos**: `ProduzirEventoAsync`, `ConsumirEventosAsync`
- **Features**:
  - Particionamento de eventos
  - Grupos de consumer
  - Offset management
  - Serialização JSON

### Endpoints para Testes
- **POST** `/api/v1/rabbit/enviar` → Envia mensagem para RabbitMQ
- **POST** `/api/v1/kafka/produzir` → Produz evento para Kafka

---

## 📊 Observabilidade

### Serilog (Logging)
- **Sink Console**: Logs estruturados no console com timestamp e level
- **Sink Seq**: Agregação de logs em http://localhost:5341
- **Enrich**: LogContext para adicionar dados contextuais
- **Níveis**: Information (default), Warning/Error para frameworks
- **Request Logging**: `app.UseSerilogRequestLogging()` loga automaticamente cada HTTP request/response

### OpenTelemetry (Tracing)
- **Exporter**: OTLP para Jaeger (http://localhost:4317)
- **Instrumentação**: AspNetCore (request/response spans)
- **Service Name**: "TL.Exemplo.API"
- **UI**: Jaeger em http://localhost:16686

### Health Checks
- **`/health`**: Status completo (database + cache + liveness)
- **`/health/live`**: Apenas liveness (aplicação está rodando)
- **`/health/ready`**: Readiness (database + cache, sem liveness)
- **Checks customizados**: SQL Server e Redis com timeouts
- **Response**: JSON com status, timestamp e detalhes de cada check

---

## 🚦 Rate Limiting

- **Middleware**: `RateLimitingMiddleware`
- **Limite**: 100 requisições por minuto por IP
- **Detecção de IP**: Suporta X-Forwarded-For (proxy awareness)
- **Response**: HTTP 429 (Too Many Requests) com JSON informando retry-after
- **Thread-safe**: Usa ConcurrentDictionary + locks para thread safety

---

## 🛠️ CQRS com MediatR

### Pattern
1. **Controller** → envia `CreateProdutoCommand` ou `GetProdutosQuery` via `_mediator.Send()`
2. **MediatR** → route para o Handler correspondente
3. **ValidationBehavior** → intercepta antes do handler, executa validators
4. **Handler** → executa lógica, retorna resultado
5. **Controller** → retorna HTTP response

### Vantagens
- ✓ Lógica desacoplada de HTTP
- ✓ Testável isoladamente
- ✓ Validações automáticas
- ✓ Reutilizável em diferentes transportes (gRPC, etc)

---

## 📝 Padrões de Resposta HTTP

### Success (200 OK, 201 Created)
```json
{
  "data": { "id": 1, "nome": "Produto A", ... },
  "mensagem": "Operação realizada com sucesso",
  "sucesso": true
}
```

### Error (400 Bad Request)
```json
{
  "mensagem": "Validação falhou",
  "erros": ["O nome é obrigatório", "O preço deve ser maior que zero"],
  "sucesso": false
}
```

### Not Found (404)
```json
{
  "mensagem": "Produto não encontrado",
  "sucesso": false
}
```

---

## 🚀 Rodando Localmente

### Pré-requisitos
- .NET 8 SDK
- Docker + Docker Compose (para infraestrutura)
- Visual Studio 2022 ou VS Code

### 1. Iniciar Infraestrutura
```bash
cd ../  # Voltar para /d/Desenvolvimento
docker-compose up -d
```

Aguarde ~30 segundos para tudo ficar ready. Services:
- **SQL Server**: localhost:1433 (sa/Estudos@2026)
- **Redis**: localhost:6379
- **RabbitMQ**: localhost:15672 (guest/guest)
- **Kafka**: localhost:9092
- **Seq**: http://localhost:5341
- **Jaeger**: http://localhost:16686
- **Adminer**: http://localhost:8080

### 2. Build & Run API
```bash
cd TL.Exemplo-VerticalSlice

# Restore NuGet packages
dotnet restore

# Build
dotnet build

# Run (Development)
dotnet run --project src/TL.Exemplo.API

# API estará em http://localhost:5000
# Swagger em http://localhost:5000/ (raiz redireciona para Swagger UI)
```

### 3. Testar Fluxo Completo

#### Login (obter JWT)
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "password"}'
```
Response: `{ "token": "eyJhbGc...", "expiresIn": 3600 }`

#### Criar Produto (com JWT)
```bash
curl -X POST http://localhost:5000/api/v1/produtos \
  -H "Authorization: Bearer <seu_jwt_aqui>" \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Notebook",
    "descricao": "Notebook de alta performance",
    "preco": 3500.00,
    "quantidadeEstoque": 10
  }'
```

#### Listar Produtos (com paginação)
```bash
curl -X GET "http://localhost:5000/api/v1/produtos/paged?pageNumber=1&pageSize=20" \
  -H "Authorization: Bearer <seu_jwt_aqui>"
```

---

## 🧪 Testes

- **Projeto**: `TL.Exemplo.Tests`
- **Framework**: xUnit (assumido, verificar csproj)
- **Executar**:
  ```bash
  dotnet test
  ```

---

## 📌 Endpoints Principais

### Autenticação
- `POST /api/v1/auth/login` → Login (sem auth)

### Produtos (requer JWT)
- `GET /api/v1/produtos` → Listar todos
- `GET /api/v1/produtos/paged` → Listar com paginação
- `GET /api/v1/produtos/{id}` → Obter por ID
- `POST /api/v1/produtos` → Criar
- `PUT /api/v1/produtos/{id}` → Atualizar
- `DELETE /api/v1/produtos/{id}` → Deletar

### Cache (requer JWT)
- `POST /api/v1/redis/gravar` → Salvar em cache
- `GET /api/v1/redis/consultar` → Recuperar do cache

### Mensageria (requer JWT)
- `POST /api/v1/rabbit/enviar` → Enviar mensagem RabbitMQ
- `POST /api/v1/kafka/produzir` → Produzir evento Kafka

### Health & Observabilidade
- `GET /health` → Status completo
- `GET /health/live` → Liveness
- `GET /health/ready` → Readiness
- Swagger: http://localhost:5000
- Seq (Logs): http://localhost:5341
- Jaeger (Traces): http://localhost:16686

---

## 🔍 Troubleshooting

### "Connection string 'SqlServer' não encontrada"
→ Verificar `appsettings.json`, SQL Server deve estar rodando em docker

### "Redis check failed"
→ Verificar se Redis está rodando: `docker-compose ps`

### "JWT SecretKey não configurada"
→ Verificar `JwtSettings:SecretKey` em `appsettings.json`

### Logs vazios
→ Verificar Seq em http://localhost:5341, Serilog envia para lá

### Traces não aparecem no Jaeger
→ Verificar se OpenTelemetry exporter está alcançando localhost:4317

---

## 📖 Referências Úteis

- **MediatR**: https://github.com/jbogard/MediatR
- **FluentValidation**: https://fluentvalidation.net/
- **Dapper**: https://github.com/DapperLib/Dapper
- **Serilog**: https://serilog.net/
- **OpenTelemetry**: https://opentelemetry.io/
- **ASP.NET Core**: https://docs.microsoft.com/aspnet/core/

---

## 📝 Notas de Desenvolvimento

- **Vertical Slice**: Cada feature é independente, pode ser desenvolvida/testada isoladamente
- **MediatR**: Central de comunicação entre camadas, evita acoplamento
- **Validação**: Automática antes do handler, reutilizável
- **Cache**: Sempre invalidar após UPDATE/DELETE
- **JWT**: Token sem info sensível, validação acontece no servidor
- **Health Checks**: Use `/health/ready` para load balancers/orchestrators
- **Rate Limiting**: Monitor o middleware para ajustar limites conforme demanda

---

*Última atualização: 30/04/2026*
