using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using TL.Exemplo.Application.Common.Models;
using TL.Exemplo.Application.Features.BackgroundJobs.Jobs;

namespace TL.Exemplo.Application.Features.BackgroundJobs.Commands;

/// <summary>
/// Comando para enfileirar um background job para execução imediata.
/// </summary>
public record EnqueueJobCommand(
    string JobType // "ProcessarNovoProduto", "SincronizarEstoque", "LimpezaCache"
) : IRequest<JobResultDto>;

/// <summary>
/// Handler que enfileira o job no Hangfire.
/// </summary>
public class EnqueueJobCommandHandler : IRequestHandler<EnqueueJobCommand, JobResultDto>
{
    private readonly Hangfire.IBackgroundJobClient _hangfireClient;
    private readonly ILogger<EnqueueJobCommandHandler> _logger;

    public EnqueueJobCommandHandler(
        Hangfire.IBackgroundJobClient hangfireClient,
        ILogger<EnqueueJobCommandHandler> logger)
    {
        _hangfireClient = hangfireClient;
        _logger = logger;
    }

    public Task<JobResultDto> Handle(EnqueueJobCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("📋 Enfileirando job: {JobType}", request.JobType);

        string jobId;

        // Mapear tipo de job para classe concreta
        jobId = request.JobType switch
        {
            "ProcessarNovoProduto" => _hangfireClient.Enqueue<ProcessarNovoProdutoJob>(
                x => x.ExecuteAsync(CancellationToken.None)),
            "SincronizarEstoque" => _hangfireClient.Enqueue<SincronizarEstoqueJob>(
                x => x.ExecuteAsync(CancellationToken.None)),
            "LimpezaCache" => _hangfireClient.Enqueue<LimpezaCacheJob>(
                x => x.ExecuteAsync(CancellationToken.None)),
            _ => throw new ApplicationException($"Tipo de job desconhecido: {request.JobType}")
        };

        _logger.LogInformation("✅ Job enfileirado com sucesso! ID: {JobId}", jobId);

        return Task.FromResult(new JobResultDto
        {
            JobId = jobId,
            JobType = request.JobType,
            Status = "Enfileirado",
            EnqueuedAt = DateTime.UtcNow
        });
    }
}
