using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TL.VerticalSlice.Template.Application.Common.Models;
using TL.VerticalSlice.Template.Application.Features.Samples.Commands.CreateSample;
using TL.VerticalSlice.Template.Application.Features.Samples.Commands.DeleteSample;
using TL.VerticalSlice.Template.Application.Features.Samples.Commands.UpdateSample;
using TL.VerticalSlice.Template.Application.Features.Samples.Queries.GetAllSamples;
using TL.VerticalSlice.Template.Application.Features.Samples.Queries.GetSampleById;
using TL.VerticalSlice.Template.Application.Features.Samples.Queries.GetSamplesPaged;

namespace TL.VerticalSlice.Template.API.Features.Samples;

/// <summary>
/// Controller responsável pelo gerenciamento de Samples.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public class SamplesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SamplesController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>
    /// Retorna todos os Samples cadastrados (sem paginação).
    /// </summary>
    /// <param name="apenasAtivos">Filtrar apenas Samples ativos.</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SampleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? apenasAtivos = null)
    {
        var result = await _mediator.Send(new GetAllSamplesQuery(apenasAtivos));
        return Ok(ApiResponse<IEnumerable<SampleDto>>.Ok(result));
    }

    /// <summary>
    /// Retorna Samples com paginação.
    /// </summary>
    /// <param name="pageNumber">Número da página (padrão: 1).</param>
    /// <param name="pageSize">Tamanho da página (padrão: 20, máximo: 100).</param>
    /// <param name="apenasAtivos">Filtrar apenas Samples ativos.</param>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SampleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? apenasAtivos = null)
    {
        var result = await _mediator.Send(new GetSamplesPagedQuery(pageNumber, pageSize, apenasAtivos));
        return Ok(ApiResponse<PagedResult<SampleDto>>.Ok(result));
    }

    /// <summary>
    /// Retorna um Sample pelo Id.
    /// </summary>
    /// <param name="id">Identificador do Sample.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SampleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetSampleByIdQuery(id));
        return Ok(ApiResponse<SampleDto>.Ok(result));
    }

    /// <summary>
    /// Cria um novo Sample.
    /// </summary>
    /// <param name="command">Dados do Sample a ser criado.</param>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SampleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSampleCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            ApiResponse<SampleDto>.Criado(result));
    }

    /// <summary>
    /// Atualiza um Sample existente.
    /// </summary>
    /// <param name="id">Identificador do Sample.</param>
    /// <param name="command">Dados atualizados do Sample.</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSampleCommand command)
    {
        if (id != command.Id)
            return BadRequest(ApiResponse<object>.Falha("O Id da rota não corresponde ao Id do corpo da requisição."));

        await _mediator.Send(command);
        return Ok(ApiResponse.Ok("Sample atualizado com sucesso."));
    }

    /// <summary>
    /// Remove um Sample pelo Id.
    /// </summary>
    /// <param name="id">Identificador do Sample.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteSampleCommand(id));
        return Ok(ApiResponse.Ok("Sample removido com sucesso."));
    }
}
