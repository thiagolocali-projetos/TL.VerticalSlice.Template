using MediatR;
using TL.Exemplo.Application.Common.Models;
using TL.Exemplo.Application.Contracts.Messaging;

namespace TL.Exemplo.Application.Features.Kafka.ConsumirEvento;

public record ConsumirEventoCommand(string Topico) : IRequest<ApiResponse<object>>;

public class ConsumirEventoHandler : IRequestHandler<ConsumirEventoCommand, ApiResponse<object>>
{
    private readonly IKafkaService _kafka;

    public ConsumirEventoHandler(IKafkaService kafka) => _kafka = kafka;

    public async Task<ApiResponse<object>> Handle(ConsumirEventoCommand request, CancellationToken ct)
    {
        await _kafka.ConsumirAsync<dynamic>(request.Topico, async msg =>
        {
            await Task.CompletedTask; // Aqui você processaria o evento
        }, ct);

        return ApiResponse<object>.Ok(new { request.Topico }, "Consumidor iniciado com sucesso!");
    }
}