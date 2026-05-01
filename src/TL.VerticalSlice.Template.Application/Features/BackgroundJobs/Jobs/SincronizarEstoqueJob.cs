using Microsoft.Extensions.Logging;
using TL.VerticalSlice.Template.Application.Contracts.Repositories;

namespace TL.VerticalSlice.Template.Application.Features.BackgroundJobs.Jobs;

/// <summary>
/// Job que sincroniza o estoque de Samples com um sistema externo (ERP, warehouse, etc).
/// Executa periodicamente (ex: a cada 5 minutos).
/// </summary>
public class SincronizarEstoqueJob : IBackgroundJob
{
    private readonly ISampleRepository _SampleRepository;
    private readonly ILogger<SincronizarEstoqueJob> _logger;

    public SincronizarEstoqueJob(
        ISampleRepository SampleRepository,
        ILogger<SincronizarEstoqueJob> logger)
    {
        _SampleRepository = SampleRepository;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ðŸ”„ Iniciando SincronizarEstoqueJob...");

        try
        {
            // Obter todos os Samples
            var Samples = await _SampleRepository.GetAllAsync();
            var SamplesList = Samples.ToList();

            if (!SamplesList.Any())
            {
                _logger.LogInformation("â„¹ï¸ Nenhum Sample para sincronizar");
                return;
            }

            _logger.LogInformation("ðŸ“¦ Sincronizando {Count} Samples com sistema externo", SamplesList.Count);

            // Simular sincronizaÃ§Ã£o com sistema externo
            foreach (var Sample in SamplesList)
            {
                // Na prÃ¡tica: chamar API do ERP, atualizar estoque, etc
                _logger.LogDebug("   â†’ Sincronizando Sample {SampleId}: {Nome}", Sample.Id, Sample.Nome);
                await Task.Delay(100, cancellationToken);
            }

            _logger.LogInformation("âœ… SincronizarEstoqueJob completado! {Count} Samples sincronizados", SamplesList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "âŒ Erro ao sincronizar estoque");
            throw;
        }
    }
}

