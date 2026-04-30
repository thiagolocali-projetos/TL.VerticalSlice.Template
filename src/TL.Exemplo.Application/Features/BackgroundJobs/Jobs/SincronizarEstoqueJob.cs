using Microsoft.Extensions.Logging;
using TL.Exemplo.Application.Contracts.Repositories;

namespace TL.Exemplo.Application.Features.BackgroundJobs.Jobs;

/// <summary>
/// Job que sincroniza o estoque de produtos com um sistema externo (ERP, warehouse, etc).
/// Executa periodicamente (ex: a cada 5 minutos).
/// </summary>
public class SincronizarEstoqueJob : IBackgroundJob
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly ILogger<SincronizarEstoqueJob> _logger;

    public SincronizarEstoqueJob(
        IProdutoRepository produtoRepository,
        ILogger<SincronizarEstoqueJob> logger)
    {
        _produtoRepository = produtoRepository;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔄 Iniciando SincronizarEstoqueJob...");

        try
        {
            // Obter todos os produtos
            var produtos = await _produtoRepository.GetAllAsync();
            var produtosList = produtos.ToList();

            if (!produtosList.Any())
            {
                _logger.LogInformation("ℹ️ Nenhum produto para sincronizar");
                return;
            }

            _logger.LogInformation("📦 Sincronizando {Count} produtos com sistema externo", produtosList.Count);

            // Simular sincronização com sistema externo
            foreach (var produto in produtosList)
            {
                // Na prática: chamar API do ERP, atualizar estoque, etc
                _logger.LogDebug("   → Sincronizando produto {ProdutoId}: {Nome}", produto.Id, produto.Nome);
                await Task.Delay(100, cancellationToken);
            }

            _logger.LogInformation("✅ SincronizarEstoqueJob completado! {Count} produtos sincronizados", produtosList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao sincronizar estoque");
            throw;
        }
    }
}
