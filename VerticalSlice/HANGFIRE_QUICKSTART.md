# Hangfire Quick Start

## 🚀 Get Started in 2 Minutes

### 1. Ensure Infrastructure is Running
```bash
cd /d/Desenvolvimento
docker-compose up -d
```
Waits for SQL Server, Redis, RabbitMQ, Kafka, Seq, Jaeger.

### 2. Start the API
```bash
cd TL.Exemplo-VerticalSlice
dotnet run --project src/TL.Exemplo.API
```

### 3. Login to Get JWT Token
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "password"}'

# Response: { "token": "eyJhbGc...", "expiresIn": 3600 }
# Save token as TOKEN variable
TOKEN="<paste_token_here>"
```

### 4. Enqueue a Job (Execute ASAP)
```bash
curl -X POST "http://localhost:5000/api/v1/backgroundjobs/enqueue?jobType=ProcessarNovoProduto" \
  -H "Authorization: Bearer $TOKEN"

# Response (202 Accepted):
# {
#   "data": {
#     "jobId": "1",
#     "status": "Enfileirado"
#   }
# }
```

### 5. Schedule a Job (Execute Later)
```bash
curl -X POST "http://localhost:5000/api/v1/backgroundjobs/schedule?jobType=SincronizarEstoque&delaySeconds=30" \
  -H "Authorization: Bearer $TOKEN"

# Executes in 30 seconds
```

### 6. View Dashboard
Open browser: **http://localhost:5000/hangfire**

See:
- Jobs en queue
- Recurring jobs
- Completed jobs
- Failed jobs (with retry button)
- Real-time logs

### 7. Check Logs
Open browser: **http://localhost:5341** (Seq)

Search: "ProcessarNovoProduto" to see execution logs

---

## 📋 Available Jobs

| Job | Type | Enqueue | Schedule | Recurring |
|-----|------|---------|----------|-----------|
| `ProcessarNovoProduto` | On-demand | ✅ | ✅ | ❌ |
| `SincronizarEstoque` | Always running | ✅ | ✅ | ✅ Every 5 min |
| `LimpezaCache` | Cleaning | ✅ | ✅ | ✅ Daily 03:00 |

---

## 🎯 Test Scenarios

### Test 1: Fire and Forget
```bash
curl -X POST "http://localhost:5000/api/v1/backgroundjobs/enqueue?jobType=ProcessarNovoProduto" \
  -H "Authorization: Bearer $TOKEN"
# Executes immediately
```

### Test 2: Delayed Execution
```bash
curl -X POST "http://localhost:5000/api/v1/backgroundjobs/schedule?jobType=ProcessarNovoProduto&delaySeconds=5" \
  -H "Authorization: Bearer $TOKEN"
# Wait 5 seconds, then executes
```

### Test 3: Watch Dashboard
1. Open http://localhost:5000/hangfire
2. Enqueue job from curl above
3. Watch job move from "Enqueued" → "Scheduled" → "Succeeded"
4. Click job to see logs and execution time

### Test 4: View Logs
1. Open http://localhost:5341 (Seq)
2. Search: `ProcessarNovoProdutoJob` 
3. See: Execution logs, duration, errors (if any)

### Test 5: Test Recurring Job
```bash
# SincronizarEstoqueJob runs every 5 minutes
# Check dashboard at http://localhost:5000/hangfire
# Under "Recurring" tab, see "sincronizar-estoque-recorrente"
# Next execution time shown
```

---

## 🔨 Create Your Own Job

### Step 1: Create Job Class
**File**: `src/TL.Exemplo.Application/Features/BackgroundJobs/Jobs/MyCustomJob.cs`

```csharp
using Microsoft.Extensions.Logging;
using TL.Exemplo.Application.Features.BackgroundJobs;

public class MyCustomJob : IBackgroundJob
{
    private readonly ILogger<MyCustomJob> _logger;

    public MyCustomJob(ILogger<MyCustomJob> logger)
        => _logger = logger;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MyCustomJob started");
        
        // Your logic here
        await Task.Delay(1000, cancellationToken);
        
        _logger.LogInformation("MyCustomJob completed");
    }
}
```

### Step 2: Register in DI
**File**: `src/TL.Exemplo.API/Extensions/HangfireExtensions.cs`

Add to `AddHangfireServices()`:
```csharp
services.AddTransient<MyCustomJob>();
```

### Step 3: Add to Commands
**Files**: 
- `EnqueueJobCommand.cs` (line 40)
- `ScheduleJobCommand.cs` (line 42)

Add case:
```csharp
"MyCustomJob" => _hangfireClient.Enqueue<MyCustomJob>(x => x.ExecuteAsync(CancellationToken.None))
```

### Step 4: Test
```bash
curl -X POST "http://localhost:5000/api/v1/backgroundjobs/enqueue?jobType=MyCustomJob" \
  -H "Authorization: Bearer $TOKEN"
```

### Step 5 (Optional): Make it Recurring
**File**: `src/TL.Exemplo.API/Extensions/HangfireExtensions.cs` → `UseHangfireConfiguration()`

Add:
```csharp
recurringJobManager.AddOrUpdate<MyCustomJob>(
    "my-custom-job",
    job => job.ExecuteAsync(CancellationToken.None),
    "0 * * * *"  // Every hour
);
```

---

## 💡 Common Tasks

### View All Jobs Status
```bash
curl http://localhost:5000/api/v1/backgroundjobs/dashboard-info \
  -H "Authorization: Bearer $TOKEN" | jq
```

### Kill a Running Job
1. Go to http://localhost:5000/hangfire
2. Find job
3. Click "Delete"

### Retry Failed Job
1. Go to http://localhost:5000/hangfire
2. Click "Failed" tab
3. Select job, click "Retry"

### View Job Execution Logs
1. Go to http://localhost:5000/hangfire
2. Click job ID
3. See "Job Details" with logs

### Change Worker Count
**File**: `src/TL.Exemplo.API/Extensions/HangfireExtensions.cs`

```csharp
services.AddHangfireServer(options =>
{
    options.WorkerCount = 8;  // Default: CPU cores * 2
```

---

## 🐛 Troubleshooting

### "Job doesn't show in dashboard"
- Check if API is running: `dotnet run --project src/TL.Exemplo.API`
- Check if SQL Server is running: `docker-compose ps`
- Refresh dashboard (F5)

### "Job failed immediately"
- Check Seq logs: http://localhost:5341
- Search job name
- See error message
- Fix code
- Retry from dashboard

### "Recurring job not executing"
- Check `HangfireExtensions.cs` for CRON expression
- Validate CRON: https://crontab.guru/
- Check dashboard "Recurring" tab for next execution time

### "Dashboard won't load"
- Ensure API is running
- Clear browser cache
- Check Network tab in DevTools
- Restart API: `dotnet run --project src/TL.Exemplo.API`

---

## 📚 Learn More

- **Complete Guide**: `VerticalSlice/HANGFIRE_GUIDE.md` (500+ lines)
- **Hangfire Docs**: https://docs.hangfire.io/
- **CRON Expressions**: https://crontab.guru/
- **SQL Server Storage**: https://docs.hangfire.io/en/latest/configuration/using-sql-server.html

---

## ✨ What You Learned

✅ Enqueue jobs for fire-and-forget execution  
✅ Schedule jobs for delayed execution  
✅ Create recurring jobs with CRON expressions  
✅ Monitor jobs in real-time dashboard  
✅ View logs in Seq  
✅ Create custom jobs  
✅ Integrate Hangfire with Vertical Slice architecture  
✅ Best practices for background job processing  

---

*Happy scheduling! 🚀*
