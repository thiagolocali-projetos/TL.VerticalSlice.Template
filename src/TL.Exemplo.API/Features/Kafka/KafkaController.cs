using MediatR;
using Microsoft.AspNetCore.Mvc;
using TL.Exemplo.Application.Features.Kafka.ProduzirEvento;
using TL.Exemplo.Application.Features.Kafka.ConsumirEvento;

namespace TL.Exemplo.API.Features.Kafka;

[ApiController]
[Route("api/v1/kafka")]
public class KafkaController : ControllerBase
{
    private readonly IMediator _mediator;

    public KafkaController(IMediator mediator) => _mediator = mediator;

    [HttpPost("produzir")]
    public async Task<IActionResult> Produzir([FromBody] ProduzirRequest request)
    {
        var cmd = new ProduzirEventoCommand(request.Topico, request.Key, request.Conteudo);
        var result = await _mediator.Send(cmd);
        return Ok(result);
    }

    [HttpPost("consumir")]
    public async Task<IActionResult> Consumir([FromBody] ConsumirRequest request)
    {
        var cmd = new ConsumirEventoCommand(request.Topico);
        var result = await _mediator.Send(cmd);
        return Ok(result);
    }
}

public record ProduzirRequest(string Topico, string Key, string Conteudo);
public record ConsumirRequest(string Topico);