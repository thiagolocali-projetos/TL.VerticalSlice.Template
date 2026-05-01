using MediatR;
using TL.VerticalSlice.Template.Application.Common.Models;
using TL.VerticalSlice.Template.Application.Contracts.Messaging;

namespace TL.VerticalSlice.Template.Application.Features.Kafka.ConsumirEvento;

public record ConsumirEventoCommand(string Topico) : IRequest<ApiResponse<object>>;

public class ConsumirEventoHandler : IRequestHandler<ConsumirEventoCommand, ApiResponse<object>>
{
    private readonly IKafkaService _kafka;

    public ConsumirEventoHandler(IKafkaService kafka) => _kafka = kafka;

    public async Task<ApiResponse<object>> Handle(ConsumirEventoCommand request, CancellationToken ct)
    {
        await _kafka.ConsumirAsync<dynamic>(request.Topico, async msg =>
        {
            await Task.CompletedTask; // Aqui vocÃª processaria o evento
        }, ct);

        return ApiResponse<object>.Ok(new { request.Topico }, "Consumidor iniciado com sucesso!");
    }
}
