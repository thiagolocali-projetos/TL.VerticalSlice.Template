using Hangfire;
using Hangfire.SqlServer;
// OPTIONAL: Uncomment the following imports when you create BackgroundJobs feature
// using TL.VerticalSlice.Template.Application.Features.BackgroundJobs;
// using TL.VerticalSlice.Template.Application.Features.BackgroundJobs.Jobs;

namespace TL.VerticalSlice.Template.API.Extensions;

/// <summary>
/// Extensions to configure Hangfire in the project.
/// </summary>
public static class HangfireExtensions
{
    /// <summary>
    /// Register Hangfire with SQL Server as storage.
    /// </summary>
    public static IServiceCollection AddHangfireServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("Connection string 'SqlServer' not configured for Hangfire");

        // Register Hangfire with SQL Server Storage
        services.AddHangfire(cfg =>
            cfg.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.FromSeconds(15),
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

        // Register Hangfire Server
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 2;
            options.Queues = new[] { "default", "critical", "background" };
        });

        // OPTIONAL: Register your background job implementations here
        // services.AddTransient<YourBackgroundJob>();

        return services;
    }

    /// <summary>
    /// Configure Hangfire Dashboard and Recurring Jobs.
    /// Must be called in the application pipeline.
    /// </summary>
    public static WebApplication UseHangfireConfiguration(this WebApplication app)
    {
        // Enable Hangfire Dashboard
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            IsReadOnlyFunc = ctx => false,
            DarkModeEnabled = true,
            DisplayStorageConnectionString = false
        });

        // Register recurring jobs
        var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

        // OPTIONAL: Register your recurring jobs here
        // Example:
        // recurringJobManager.AddOrUpdate<YourBackgroundJob>(
        //     "your-job-id",
        //     job => job.ExecuteAsync(CancellationToken.None),
        //     "*/5 * * * *"  // CRON expression: every 5 minutes
        // );

        return app;
    }
}
