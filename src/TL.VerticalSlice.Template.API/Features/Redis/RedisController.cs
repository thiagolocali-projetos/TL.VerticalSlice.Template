using MediatR;
using Microsoft.AspNetCore.Mvc;
using TL.VerticalSlice.Template.Application.Features.Cache.GravarCache;
using TL.VerticalSlice.Template.Application.Features.Cache.ConsultarCache;

namespace TL.VerticalSlice.Template.API.Features.Redis;

[ApiController]
[Route("api/v1/cache")]
public class RedisController : ControllerBase
{
    private readonly IMediator _mediator;

    public RedisController(IMediator mediator) => _mediator = mediator;

    [HttpPost("gravar")]
    public async Task<IActionResult> Gravar([FromBody] GravarCacheRequest request)
    {
        var cmd = new GravarCacheCommand(request.Key, request.Valor, request.MinutosExpiracao);
        var result = await _mediator.Send(cmd);
        return Ok(result);
    }

    [HttpGet("consultar/{key}")]
    public async Task<IActionResult> Consultar(string key)
    {
        var cmd = new ConsultarCacheCommand(key);
        var result = await _mediator.Send(cmd);
        return Ok(result);
    }
}

public record GravarCacheRequest(string Key, string Valor, int? MinutosExpiracao = 5);
