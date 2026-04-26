using MediatR;
using TL.Exemplo.Application.Common.Models;
using TL.Exemplo.Application.Contracts.Messaging;

namespace TL.Exemplo.Application.Features.Kafka.ProduzirEvento;

public record ProduzirEventoCommand(string Topico, string Key, string Conteudo) : IRequest<ApiResponse<object>>;

public class ProduzirEventoHandler : IRequestHandler<ProduzirEventoCommand, ApiResponse<object>>
{
    private readonly IKafkaService _kafka;

    public ProduzirEventoHandler(IKafkaService kafka) => _kafka = kafka;

    public async Task<ApiResponse<object>> Handle(ProduzirEventoCommand request, CancellationToken ct)
    {
        await _kafka.ProduzirAsync(request.Topico, request.Key, new { request.Conteudo, Data = DateTime.UtcNow }, ct);
        return ApiResponse<object>.Ok(new { request.Topico, request.Key }, "Evento produzido com sucesso!");
    }
}