using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using TL.Exemplo.Application.Common.Models;
using TL.Exemplo.Application.Features.BackgroundJobs.Jobs;

namespace TL.Exemplo.Application.Features.BackgroundJobs.Commands;

/// <summary>
/// Comando para agendar um background job para execução em tempo futuro.
/// </summary>
public record ScheduleJobCommand(
    string JobType,                    // "ProcessarNovoProduto", etc
    int DelaySeconds                   // Segundos até execução
) : IRequest<JobResultDto>;

/// <summary>
/// Handler que agenda o job no Hangfire com delay.
/// </summary>
public class ScheduleJobCommandHandler : IRequestHandler<ScheduleJobCommand, JobResultDto>
{
    private readonly Hangfire.IBackgroundJobClient _hangfireClient;
    private readonly ILogger<ScheduleJobCommandHandler> _logger;

    public ScheduleJobCommandHandler(
        Hangfire.IBackgroundJobClient hangfireClient,
        ILogger<ScheduleJobCommandHandler> logger)
    {
        _hangfireClient = hangfireClient;
        _logger = logger;
    }

    public Task<JobResultDto> Handle(ScheduleJobCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "⏱️ Agendando job: {JobType} para {Seconds} segundos no futuro",
            request.JobType,
            request.DelaySeconds);

        var delay = TimeSpan.FromSeconds(request.DelaySeconds);

        string jobId = request.JobType switch
        {
            "ProcessarNovoProduto" => _hangfireClient.Schedule<ProcessarNovoProdutoJob>(
                x => x.ExecuteAsync(CancellationToken.None),
                delay),
            "SincronizarEstoque" => _hangfireClient.Schedule<SincronizarEstoqueJob>(
                x => x.ExecuteAsync(CancellationToken.None),
                delay),
            "LimpezaCache" => _hangfireClient.Schedule<LimpezaCacheJob>(
                x => x.ExecuteAsync(CancellationToken.None),
                delay),
            _ => throw new ApplicationException($"Tipo de job desconhecido: {request.JobType}")
        };

        var scheduledTime = DateTime.UtcNow.AddSeconds(request.DelaySeconds);

        _logger.LogInformation(
            "✅ Job agendado com sucesso! ID: {JobId}, Execução em: {ScheduledTime:O}",
            jobId,
            scheduledTime);

        return Task.FromResult(new JobResultDto
        {
            JobId = jobId,
            JobType = request.JobType,
            Status = "Agendado",
            EnqueuedAt = DateTime.UtcNow,
            ScheduledFor = scheduledTime
        });
    }
}
