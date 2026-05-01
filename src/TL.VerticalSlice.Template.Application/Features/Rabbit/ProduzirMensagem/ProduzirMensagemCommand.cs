using MediatR;
using TL.VerticalSlice.Template.Application.Common.Models;
using TL.VerticalSlice.Template.Application.Contracts.Messaging;

namespace TL.VerticalSlice.Template.Application.Features.Rabbit.ProduzirMensagem;

public record ProduzirMensagemCommand(string Fila, string Conteudo) : IRequest<ApiResponse<object>>;

public class ProduzirMensagemHandler : IRequestHandler<ProduzirMensagemCommand, ApiResponse<object>>
{
    private readonly IRabbitMqService _rabbit;
    public ProduzirMensagemHandler(IRabbitMqService rabbit) => _rabbit = rabbit;

    public async Task<ApiResponse<object>> Handle(ProduzirMensagemCommand request, CancellationToken ct)
    {
        await _rabbit.ProduzirAsync(request.Fila, new { request.Conteudo, Data = DateTime.UtcNow });
        return ApiResponse<object>.Ok(new { request.Fila }, "Mensagem publicada");
    }
}
