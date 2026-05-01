using Microsoft.Extensions.Logging;
using TL.VerticalSlice.Template.Application.Contracts.Cache;

namespace TL.VerticalSlice.Template.Application.Features.BackgroundJobs.Jobs;

/// <summary>
/// Job que executa limpeza periÃ³dica de cache.
/// Remove entradas antigas ou padrÃµes especÃ­ficos (ex: "session:*", "temp:*").
/// </summary>
public class LimpezaCacheJob : IBackgroundJob
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<LimpezaCacheJob> _logger;

    public LimpezaCacheJob(
        ICacheService cacheService,
        ILogger<LimpezaCacheJob> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ðŸ§¹ Iniciando LimpezaCacheJob...");

        try
        {
            // Simular remoÃ§Ã£o de chaves expiradas
            var keysToRemove = new[] { "Sample:cache:temp", "session:expired", "temp:*" };

            _logger.LogInformation("ðŸ—‘ï¸ Removendo {Count} padrÃµes de cache", keysToRemove.Length);

            foreach (var key in keysToRemove)
            {
                try
                {
                    await _cacheService.RemoveAsync(key);
                    _logger.LogDebug("   â†’ Removido: {Key}", key);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "   âš ï¸ Falha ao remover: {Key}", key);
                }
            }

            _logger.LogInformation("âœ… LimpezaCacheJob completado!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "âŒ Erro ao limpar cache");
            throw;
        }
    }
}

