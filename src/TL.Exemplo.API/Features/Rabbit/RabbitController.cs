using MediatR;
using Microsoft.AspNetCore.Mvc;
using TL.Exemplo.Application.Features.Rabbit.ConsumirMensagem;
using TL.Exemplo.Application.Features.Rabbit.ProduzirMensagem;

namespace TL.Exemplo.API.Features.Rabbit;

[ApiController]
[Route("api/v1/rabbit")]
public class RabbitController : ControllerBase
{
    private readonly IMediator _mediator;

    public RabbitController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Publica uma mensagem na fila RabbitMQ
    /// </summary>
    [HttpPost("produzir")]
    public async Task<IActionResult> Produzir(
        [FromBody] ProduzirMensagemRequest request
        )
    {
        var cmd = new ProduzirMensagemCommand(request.Fila, request.Conteudo);
        var result = await _mediator.Send(cmd);
        return Ok(result);
    }

    /// <summary>
    /// Inicia o consumidor para escutar a fila RabbitMQ
    /// </summary>
    [HttpPost("consumir")]
    public async Task<IActionResult> Consumir(
        [FromBody] ConsumirMensagemRequest request
        )
    {
        var cmd = new ConsumirMensagemCommand(request.Fila);
        var result = await _mediator.Send(cmd);
        return Ok(result);
    }
}

public record ProduzirMensagemRequest(string Fila, string Conteudo);
public record ConsumirMensagemRequest(string Fila);