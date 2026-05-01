using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace TL.VerticalSlice.Template.API.Extensions;

/// <summary>
/// Extensões para configurar health checks da aplicação.
/// </summary>
public static class HealthCheckExtensions
{
    public static IServiceCollection AddApplicationHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // SQL Server Health Check customizado
        var sqlConnectionString = configuration.GetConnectionString("SqlServer");
        if (!string.IsNullOrEmpty(sqlConnectionString))
        {
            healthChecksBuilder.AddCheck(
                "SQL Server",
                () => CheckSqlServer(sqlConnectionString),
                tags: new[] { "database", "sql" }
            );
        }

        // Redis Health Check customizado
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            healthChecksBuilder.AddCheck(
                "Redis Cache",
                () => CheckRedis(redisConnectionString),
                tags: new[] { "cache", "redis" }
            );
        }

        // Liveness probe (aplicação está rodando)
        healthChecksBuilder.AddCheck(
            "Liveness",
            () => HealthCheckResult.Healthy("Application is alive"),
            tags: new[] { "live" }
        );

        return services;
    }

    public static WebApplication MapApplicationHealthChecks(this WebApplication app)
    {
        // Endpoint padrão de health check
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = WriteJsonResponse
        });

        // Endpoint de readiness (ready para receber requisições)
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => !check.Tags.Contains("live"),
            ResponseWriter = WriteJsonResponse
        });

        // Endpoint de liveness (apenas verificar se está vivo)
        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = WriteJsonResponse
        });

        return app;
    }

    /// <summary>
    /// Verifica a conectividade com SQL Server.
    /// </summary>
    private static HealthCheckResult CheckSqlServer(string connectionString)
    {
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT 1";
                    var result = command.ExecuteScalar();
                    return HealthCheckResult.Healthy("SQL Server is reachable");
                }
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"SQL Server check failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica a conectividade com Redis.
    /// </summary>
    private static HealthCheckResult CheckRedis(string connectionString)
    {
        try
        {
            var connection = ConnectionMultiplexer.Connect(connectionString);
            var server = connection.GetServer(connection.GetEndPoints().First());
            server.Ping();
            return HealthCheckResult.Healthy("Redis is reachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Redis check failed: {ex.Message}");
        }
    }

    private static async Task WriteJsonResponse(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                exception = entry.Value.Exception?.Message
            })
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
