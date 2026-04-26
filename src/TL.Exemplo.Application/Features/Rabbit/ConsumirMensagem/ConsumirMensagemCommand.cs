using MediatR;
using TL.Exemplo.Application.Common.Models;
using TL.Exemplo.Application.Contracts.Messaging;

namespace TL.Exemplo.Application.Features.Rabbit.ProduzirMensagem;

public record ProduzirMensagemCommand(string Fila, string Conteudo) : IRequest<ApiResponse<object>>;

public class PublicarMensagemHandler : IRequestHandler<ProduzirMensagemCommand, ApiResponse<object>>
{
    private readonly IRabbitMqService _rabbit;
    public PublicarMensagemHandler(IRabbitMqService rabbit) => _rabbit = rabbit;

    public async Task<ApiResponse<object>> Handle(ProduzirMensagemCommand request, CancellationToken ct)
    {
        await _rabbit.ProduzirAsync(request.Fila, new { request.Conteudo, Data = DateTime.UtcNow });
        return ApiResponse<object>.Ok(new { request.Fila }, "Mensagem publicada");
    }
}