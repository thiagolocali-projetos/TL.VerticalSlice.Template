using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TL.VerticalSlice.Template.Application.Common.Models;
using TL.VerticalSlice.Template.Application.Features.BackgroundJobs.Commands;

namespace TL.VerticalSlice.Template.API.Features.BackgroundJobs;

/// <summary>
/// Controller para gerenciar background jobs via Hangfire.
/// Permite enfileirar e agendar jobs para execuÃ§Ã£o assÃ­ncrona.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public class BackgroundJobsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BackgroundJobsController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>
    /// Enfileira um background job para execuÃ§Ã£o imediata (ASAP).
    /// </summary>
    /// <param name="jobType">Tipo de job: "ProcessarNovoSample", "SincronizarEstoque", "LimpezaCache"</param>
    [HttpPost("enqueue")]
    [ProducesResponseType(typeof(ApiResponse<JobResultDto>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnqueueJob([FromQuery] string jobType)
    {
        var command = new EnqueueJobCommand(jobType);
        var result = await _mediator.Send(command);

        return new AcceptedResult(
            (string?)null,
            ApiResponse<JobResultDto>.Ok(result, "Job enfileirado com sucesso. Verificar status em /hangfire dashboard.")
        );
    }

    /// <summary>
    /// Agenda um background job para execuÃ§Ã£o em tempo futuro.
    /// </summary>
    /// <param name="jobType">Tipo de job: "ProcessarNovoSample", "SincronizarEstoque", "LimpezaCache"</param>
    /// <param name="delaySeconds">Quantos segundos aguardar antes de executar (mÃ­n: 1, mÃ¡x: 86400)</param>
    [HttpPost("schedule")]
    [ProducesResponseType(typeof(ApiResponse<JobResultDto>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ScheduleJob([FromQuery] string jobType, [FromQuery] int delaySeconds)
    {
        if (delaySeconds < 1 || delaySeconds > 86400)
            return BadRequest(ApiResponse<object>.Falha("DelaySeconds deve estar entre 1 e 86400 segundos"));

        var command = new ScheduleJobCommand(jobType, delaySeconds);
        var result = await _mediator.Send(command);

        return new AcceptedResult(
            (string?)null,
            ApiResponse<JobResultDto>.Ok(result, $"Job agendado para execuÃ§Ã£o em {result.ScheduledFor:O}")
        );
    }

    /// <summary>
    /// Retorna informaÃ§Ãµes sobre o Hangfire Dashboard.
    /// </summary>
    [HttpGet("dashboard-info")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult GetDashboardInfo()
    {
        var info = new
        {
            dashboardUrl = "/hangfire",
            description = "Acesse o Hangfire Dashboard para monitorar jobs em tempo real",
            availableJobs = new[]
            {
                new { name = "ProcessarNovoSample", description = "Processa novo Sample (ex: enviar email)" },
                new { name = "SincronizarEstoque", description = "Sincroniza estoque com sistema externo (executa a cada 5 min)" },
                new { name = "LimpezaCache", description = "Limpa entradas antigas de cache (executa diariamente Ã s 03:00)" }
            },
            examples = new
            {
                enqueueNow = "POST /api/v1/backgroundjobs/enqueue?jobType=SincronizarEstoque",
                scheduleIn30Seconds = "POST /api/v1/backgroundjobs/schedule?jobType=ProcessarNovoSample&delaySeconds=30"
            }
        };

        return Ok(ApiResponse<object>.Ok(info, "InformaÃ§Ãµes sobre Background Jobs"));
    }
}

