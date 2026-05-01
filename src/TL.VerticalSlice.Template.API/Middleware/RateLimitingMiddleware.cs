using System.Collections.Concurrent;
using System.Net;

namespace TL.VerticalSlice.Template.API.Middleware;

/// <summary>
/// Middleware para implementar rate limiting baseado em IP.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly int _requestsPerMinute;
    private static readonly ConcurrentDictionary<string, RequestHistory> RequestHistories = new();

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, int requestsPerMinute = 100)
    {
        _next = next;
        _logger = logger;
        _requestsPerMinute = requestsPerMinute;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIp(context);
        var now = DateTime.UtcNow;

        var history = RequestHistories.AddOrUpdate(
            clientIp,
            new RequestHistory(),
            (key, existing) =>
            {
                // Limpar requisiÃ§Ãµes antigas (anterior a 1 minuto)
                existing.Requests.RemoveWhere(r => (now - r).TotalSeconds > 60);
                return existing;
            }
        );

        bool isRateLimited = false;

        // Usar lock apenas para operaÃ§Ãµes sÃ­ncronas
        lock (history)
        {
            history.Requests.RemoveWhere(r => (now - r).TotalSeconds > 60);

            if (history.Requests.Count >= _requestsPerMinute)
            {
                isRateLimited = true;
            }
            else
            {
                history.Requests.Add(now);
            }
        }

        if (isRateLimited)
        {
            _logger.LogWarning("âš ï¸ Rate limit exceeded para IP: {ClientIp}", clientIp);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Rate limit exceeded",
                message = $"VocÃª excedeu o limite de {_requestsPerMinute} requisiÃ§Ãµes por minuto",
                retryAfter = 60
            };

            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        await _next(context);
    }

    private static string GetClientIp(HttpContext context)
    {
        // Tentar obter do header X-Forwarded-For (quando atrÃ¡s de proxy)
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ips = forwardedFor.ToString().Split(',');
            return ips[0].Trim();
        }

        // Usar RemoteIpAddress como fallback
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Classe auxiliar para rastrear histÃ³rico de requisiÃ§Ãµes.
    /// </summary>
    private class RequestHistory
    {
        public HashSet<DateTime> Requests { get; } = new();
    }
}

/// <summary>
/// ExtensÃµes para registrar o middleware de rate limiting.
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(
        this IApplicationBuilder app,
        int requestsPerMinute = 100)
    {
        return app.UseMiddleware<RateLimitingMiddleware>(requestsPerMinute);
    }
}

