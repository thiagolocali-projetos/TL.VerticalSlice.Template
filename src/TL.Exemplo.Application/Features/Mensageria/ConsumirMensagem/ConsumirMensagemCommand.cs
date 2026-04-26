using MediatR;
using TL.Exemplo.Application.Common.Models;
using TL.Exemplo.Application.Contracts.Messaging;

namespace TL.Exemplo.Application.Features.Mensageria.PublicarMensagem;

public record PublicarMensagemCommand(string Fila, string Conteudo) : IRequest<ApiResponse<object>>;

public class PublicarMensagemHandler : IRequestHandler<PublicarMensagemCommand, ApiResponse<object>>
{
    private readonly IRabbitMqService _rabbit;
    public PublicarMensagemHandler(IRabbitMqService rabbit) => _rabbit = rabbit;

    public async Task<ApiResponse<object>> Handle(PublicarMensagemCommand request, CancellationToken ct)
    {
        await _rabbit.PublicarAsync(request.Fila, new { request.Conteudo, Data = DateTime.UtcNow });
        return ApiResponse<object>.Ok(new { request.Fila }, "Mensagem publicada");
    }
}