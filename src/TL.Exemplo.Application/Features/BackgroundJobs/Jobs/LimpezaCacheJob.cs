using Microsoft.Extensions.Logging;
using TL.Exemplo.Application.Contracts.Cache;

namespace TL.Exemplo.Application.Features.BackgroundJobs.Jobs;

/// <summary>
/// Job que executa limpeza periódica de cache.
/// Remove entradas antigas ou padrões específicos (ex: "session:*", "temp:*").
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
        _logger.LogInformation("🧹 Iniciando LimpezaCacheJob...");

        try
        {
            // Simular remoção de chaves expiradas
            var keysToRemove = new[] { "produto:cache:temp", "session:expired", "temp:*" };

            _logger.LogInformation("🗑️ Removendo {Count} padrões de cache", keysToRemove.Length);

            foreach (var key in keysToRemove)
            {
                try
                {
                    await _cacheService.RemoveAsync(key);
                    _logger.LogDebug("   → Removido: {Key}", key);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "   ⚠️ Falha ao remover: {Key}", key);
                }
            }

            _logger.LogInformation("✅ LimpezaCacheJob completado!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao limpar cache");
            throw;
        }
    }
}
