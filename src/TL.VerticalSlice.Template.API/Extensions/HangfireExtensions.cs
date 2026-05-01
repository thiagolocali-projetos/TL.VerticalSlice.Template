using Hangfire;
using Hangfire.SqlServer;
using TL.VerticalSlice.Template.Application.Features.BackgroundJobs;
using TL.VerticalSlice.Template.Application.Features.BackgroundJobs.Jobs;

namespace TL.VerticalSlice.Template.API.Extensions;

/// <summary>
/// ExtensÃµes para configurar Hangfire no projeto.
/// </summary>
public static class HangfireExtensions
{
    /// <summary>
    /// Registra Hangfire com SQL Server como storage.
    /// </summary>
    public static IServiceCollection AddHangfireServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("Connection string 'SqlServer' nÃ£o configurada para Hangfire");

        // Registrar Hangfire com SQL Server Storage
        services.AddHangfire(cfg =>
            cfg.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.FromSeconds(15),
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

        // Registrar Hangfire Server
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 2;
            options.Queues = new[] { "default", "critical", "background" };
        });

        // Registrar Jobs como Transient (novo a cada execuÃ§Ã£o)
        services.AddTransient<ProcessarNovoSampleJob>();
        services.AddTransient<SincronizarEstoqueJob>();
        services.AddTransient<LimpezaCacheJob>();

        return services;
    }

    /// <summary>
    /// Configura Hangfire Dashboard e Recurring Jobs.
    /// Deve ser chamado no pipeline da aplicaÃ§Ã£o.
    /// </summary>
    public static WebApplication UseHangfireConfiguration(this WebApplication app)
    {
        // Habilitar Hangfire Dashboard
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            IsReadOnlyFunc = ctx => false,  // Permitir aÃ§Ãµes no dashboard
            DarkModeEnabled = true,          // Tema escuro
            DisplayStorageConnectionString = false  // NÃ£o exibir connection string
        });

        // Agendar Recurring Jobs
        var backgroundJobClient = app.Services.GetRequiredService<IBackgroundJobClient>();
        var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

        // Recurring jobs - temporarily disabled during startup
        try
        {
            // Job que executa a cada 5 minutos (*/5 * * * *)
            recurringJobManager.AddOrUpdate<SincronizarEstoqueJob>(
                "sincronizar-estoque-recorrente",
                job => job.ExecuteAsync(CancellationToken.None),
                "*/5 * * * *"  // CRON expression: a cada 5 minutos
            );

            // Job que executa diariamente Ã s 03:00 (0 3 * * *)
            recurringJobManager.AddOrUpdate<LimpezaCacheJob>(
                "limpeza-cache-diaria",
                job => job.ExecuteAsync(CancellationToken.None),
                "0 3 * * *"  // CRON expression: diariamente Ã s 03:00 UTC
            );
        }
        catch (Exception ex)
        {
            // Log but don't crash - database might not be ready yet
            Console.WriteLine($"âš ï¸ Warning: Could not register recurring jobs: {ex.Message}");
        }

        return app;
    }
}

