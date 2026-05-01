using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using TL.VerticalSlice.Template.Application.Common.Models;
using TL.VerticalSlice.Template.Application.Features.BackgroundJobs.Jobs;

namespace TL.VerticalSlice.Template.Application.Features.BackgroundJobs.Commands;

/// <summary>
/// Comando para agendar um background job para execuÃ§Ã£o em tempo futuro.
/// </summary>
public record ScheduleJobCommand(
    string JobType,                    // "ProcessarNovoSample", etc
    int DelaySeconds                   // Segundos atÃ© execuÃ§Ã£o
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
            "â±ï¸ Agendando job: {JobType} para {Seconds} segundos no futuro",
            request.JobType,
            request.DelaySeconds);

        var delay = TimeSpan.FromSeconds(request.DelaySeconds);

        string jobId = request.JobType switch
        {
            "ProcessarNovoSample" => _hangfireClient.Schedule<ProcessarNovoSampleJob>(
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
            "âœ… Job agendado com sucesso! ID: {JobId}, ExecuÃ§Ã£o em: {ScheduledTime:O}",
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

