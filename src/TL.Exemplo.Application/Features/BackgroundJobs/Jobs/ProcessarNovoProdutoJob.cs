using Microsoft.Extensions.Logging;

namespace TL.Exemplo.Application.Features.BackgroundJobs.Jobs;

/// <summary>
/// Job que simula o processamento de um novo produto.
/// Na prática, poderia: enviar email de notificação, gerar thumbnail de imagem, indexar em busca, etc.
/// </summary>
public class ProcessarNovoProdutoJob : IBackgroundJob
{
    private readonly ILogger<ProcessarNovoProdutoJob> _logger;

    public ProcessarNovoProdutoJob(ILogger<ProcessarNovoProdutoJob> logger)
        => _logger = logger;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔄 Iniciando ProcessarNovoProdutoJob...");

        try
        {
            // Simular processamento
            await Task.Delay(2000, cancellationToken);

            _logger.LogInformation("✅ ProcessarNovoProdutoJob completado com sucesso!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao processar novo produto");
            throw;
        }
    }
}
