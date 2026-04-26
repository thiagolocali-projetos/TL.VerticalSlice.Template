using MediatR;
using Microsoft.AspNetCore.Mvc;
using TL.Exemplo.Application.Features.Rabbit.ConsumirMensagem;
using TL.Exemplo.Application.Features.Rabbit.PublicarMensagem;

namespace TL.Exemplo.API.Features.Rabbit;

[ApiController]
[Route("api/v1/rabbit")]
public class RabbitController : ControllerBase
{
    private readonly IMediator _mediator;
    public RabbitController(IMediator mediator) => _mediator = mediator;

    [HttpPost("produzir")]
    public async Task<IActionResult> Produzir([FromBody] ProduzirRequest request)
    {
        var cmd = new ProduzirMensagemCommand(request.Fila, request.Conteudo);
        var result = await _mediator.Send(cmd);
        return Ok(result);
    }

    [HttpPost("consumir")]
    public async Task<IActionResult> Consumir([FromBody] ConsumirRequest request)
    {
        var cmd = new ConsumirMensagemCommand(request.Fila);
        var result = await _mediator.Send(cmd);
        return Ok(result);
    }
}

public record ProduzirRequest(string Fila, string Conteudo);
public record ConsumirRequest(string Fila);