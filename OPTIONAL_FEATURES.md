# Funcionalidades Opcionais - TL.VerticalSlice.Template

Este template inclui suporte para funcionalidades avançadas que podem ser ativadas conforme necessário. Por padrão, o template é entregue com apenas autenticação e exemplo CRUD.

## Features Disponíveis

### 1. ✅ Cache com Redis (Recomendado)

**Status:** Implementação pronta, controller opcional

**O que já está incluído:**
- `RedisCacheService` em `Infrastructure/Cache/`
- Configuração no `Program.cs` (comentada)
- Modelo de uso em `Application/Features/Cache/`

**Como ativar:**

1. Descomente em `Program.cs`:
```csharp
// Redis Cache (UNCOMMENT TO ENABLE)
// services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
//     options.InstanceName = "MyProject_";
// });
// services.AddSingleton<ICacheService, RedisCacheService>();
```

2. Adicione connection string em `appsettings.json`:
```json
"ConnectionStrings": {
    "Redis": "localhost:6379"
}
```

3. No `docker-compose.yml`, descomente ou adicione:
```yaml
redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
```

4. Use nos handlers:
```csharp
public class YourHandler : IRequestHandler<YourQuery, YourDto>
{
    private readonly ICacheService _cacheService;
    
    public async Task<YourDto> Handle(YourQuery request, CancellationToken ct)
    {
        var cached = await _cacheService.GetAsync<YourDto>("key", ct);
        if (cached != null) return cached;
        
        // ... seu código
        
        await _cacheService.SetAsync("key", result, TimeSpan.FromMinutes(5), ct);
        return result;
    }
}
```

---

### 2. 📨 RabbitMQ (Message Queue)

**Status:** Implementação pronta (remover controller antes, adicionar handlers conforme necessário)

**O que já estava incluído:**
- `RabbitMqService` em `Infrastructure/Messaging/`
- Configuração e exemplos documentados

**Como ativar:**

1. Descomente em `Program.cs`:
```csharp
// RabbitMQ (UNCOMMENT TO ENABLE)
// services.AddSingleton<IRabbitMqService, RabbitMqService>();
```

2. Adicione em `appsettings.json`:
```json
"RabbitMq": {
    "HostName": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
}
```

3. No `docker-compose.yml`:
```yaml
rabbitmq:
    image: rabbitmq:3-management-alpine
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
```

4. Crie suas features (exemplo):
```csharp
// src/TL.YourProject.Application/Features/Orders/Commands/PublishOrderCreated/

public record PublishOrderCreatedCommand(int OrderId, string CustomerName) : IRequest;

public class PublishOrderCreatedHandler : IRequestHandler<PublishOrderCreatedCommand>
{
    private readonly IRabbitMqService _rabbitMq;
    
    public async Task Handle(PublishOrderCreatedCommand request, CancellationToken ct)
    {
        await _rabbitMq.PublishAsync("orders.created", new
        {
            OrderId = request.OrderId,
            CustomerName = request.CustomerName
        });
    }
}
```

---

### 3. 🚀 Kafka (Event Streaming)

**Status:** Implementação pronta (remover controller antes, adicionar handlers conforme necessário)

**O que já estava incluído:**
- `KafkaService` em `Infrastructure/Messaging/`
- Configuração e exemplos documentados

**Como ativar:**

1. Descomente em `Program.cs`:
```csharp
// Kafka (UNCOMMENT TO ENABLE)
// services.AddSingleton<IKafkaService, KafkaService>();
```

2. Adicione em `appsettings.json`:
```json
"Kafka": {
    "BootstrapServers": "localhost:9092"
}
```

3. No `docker-compose.yml`:
```yaml
zookeeper:
    image: confluentinc/cp-zookeeper:7.5.0
    environment:
      ZOOKEEPER_CLIENT_PORT: 2181
    ports:
      - "2181:2181"

kafka:
    image: confluentinc/cp-kafka:7.5.0
    depends_on:
      - zookeeper
    ports:
      - "9092:9092"
    environment:
      KAFKA_BROKER_ID: 1
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:29092,PLAINTEXT_HOST://kafka:9092
      KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT
      KAFKA_INTER_BROKER_LISTENER_NAME: PLAINTEXT
```

4. Use para publicar eventos:
```csharp
public class PublishEventHandler : IRequestHandler<PublishEventCommand>
{
    private readonly IKafkaService _kafka;
    
    public async Task Handle(PublishEventCommand request, CancellationToken ct)
    {
        await _kafka.PublishAsync("my-topic", new { Id = request.Id, Data = request.Data });
    }
}
```

---

### 4. ⏰ Background Jobs com Hangfire

**Status:** Integração pronta (registrada no Program.cs)

**O que já está incluído:**
- `AddHangfireServices()` em `Program.cs`
- Suporte para jobs recorrentes e agendados
- Dashboard em `/hangfire`

**Como ativar:**

Jobs estão habilitados por padrão. Use assim:

```csharp
// Registre seus jobs em uma classe estática
public static class HangfireJobs
{
    public static void RegisterRecurringJobs()
    {
        RecurringJob.AddOrUpdate<ISyncService>(
            "sync-inventory",
            service => service.SyncInventoryAsync(),
            Cron.Daily(2) // 2am daily
        );
    }
}

// Em Program.cs, após `var app = builder.Build();`
HangfireJobs.RegisterRecurringJobs();
```

**Dashboard:**
```
http://localhost:5001/hangfire
```

---

### 5. 📊 Observabilidade (Seq + Jaeger)

**Status:** Pré-configurado no template

- **Seq** (Structured Logging): http://localhost:5341
- **Jaeger** (Distributed Tracing): http://localhost:16686

Já estão ativados em `Program.cs`. Para desativar, comente:

```csharp
// Em Program.cs, nas configurações de Serilog:
.WriteTo.Seq("http://localhost:5341")  // Remover se não usar

// Em AddOpenTelemetry():
.AddOtlpExporter(o => o.Endpoint = new Uri("http://localhost:4317"))  // Remover se não usar
```

---

## Resumo de Ativação

| Feature | Ativado? | Como Ativar |
|---------|----------|------------|
| **Auth** | ✅ Padrão | - |
| **Sample CRUD** | ✅ Padrão | - |
| **Redis Cache** | ❌ Comentado | Descomente `Program.cs` + docker |
| **RabbitMQ** | ❌ Removido | Adicione handlers conforme necessário |
| **Kafka** | ❌ Removido | Adicione handlers conforme necessário |
| **Hangfire** | ✅ Padrão | Use `RecurringJob` ou `BackgroundJob` |
| **Seq + Jaeger** | ✅ Padrão | Descomente `docker-compose.yml` |

---

## Exemplo: Ativando Redis + Hangfire

1. Descomente Redis em `Program.cs`
2. Descomente `redis` em `docker-compose.yml`
3. Execute:
```bash
docker-compose up -d
dotnet run --project src/TL.YourProject.API/
```

4. Use nos handlers:
```csharp
public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly ICacheService _cache;
    private readonly IUserRepository _repo;
    private readonly IBackgroundJobClient _backgroundJobs;
    
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        // Try cache first
        var cached = await _cache.GetAsync<UserDto>($"user:{request.Id}", ct);
        if (cached != null) return cached;
        
        // Load from DB
        var user = await _repo.GetByIdAsync(request.Id, ct);
        
        // Cache for 1 hour
        await _cache.SetAsync($"user:{request.Id}", user, TimeSpan.FromHours(1), ct);
        
        // Schedule cleanup job
        _backgroundJobs.Schedule<IUserCleanupService>(
            service => service.CleanupOldCacheAsync(),
            TimeSpan.FromDays(1)
        );
        
        return user;
    }
}
```

---

## Troubleshooting

**Redis não conecta:**
```bash
docker-compose up -d redis
docker logs redis  # Ver logs
```

**RabbitMQ management UI:**
```
http://localhost:15672
Username: guest
Password: guest
```

**Hangfire dashboard:**
```
http://localhost:5001/hangfire
```

**Jaeger traces:**
```
http://localhost:16686
```

---

**Dúvidas?** Consulte os exemplos nas pastas `src/TL.VerticalSlice.Template.*/Features/`
