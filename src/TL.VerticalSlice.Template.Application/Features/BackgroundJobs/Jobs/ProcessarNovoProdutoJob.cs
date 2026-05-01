using Microsoft.Extensions.Logging;

namespace TL.VerticalSlice.Template.Application.Features.BackgroundJobs.Jobs;

/// <summary>
/// Job que simula o processamento de um novo Sample.
/// Na prÃ¡tica, poderia: enviar email de notificaÃ§Ã£o, gerar thumbnail de imagem, indexar em busca, etc.
/// </summary>
public class ProcessarNovoSampleJob : IBackgroundJob
{
    private readonly ILogger<ProcessarNovoSampleJob> _logger;

    public ProcessarNovoSampleJob(ILogger<ProcessarNovoSampleJob> logger)
        => _logger = logger;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ðŸ”„ Iniciando ProcessarNovoSampleJob...");

        try
        {
            // Simular processamento
            await Task.Delay(2000, cancellationToken);

            _logger.LogInformation("âœ… ProcessarNovoSampleJob completado com sucesso!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "âŒ Erro ao processar novo Sample");
            throw;
        }
    }
}

