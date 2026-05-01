using MediatR;
using TL.VerticalSlice.Template.Application.Common.Models;
using TL.VerticalSlice.Template.Application.Contracts.Messaging;

namespace TL.VerticalSlice.Template.Application.Features.Rabbit.ConsumirMensagem;

public record ConsumirMensagemCommand(string Fila) : IRequest<ApiResponse<object>>;

public class ConsumirMensagemHandler : IRequestHandler<ConsumirMensagemCommand, ApiResponse<object>>
{
    private readonly IRabbitMqService _rabbit;
    public ConsumirMensagemHandler(IRabbitMqService rabbit) => _rabbit = rabbit;

    public async Task<ApiResponse<object>> Handle(ConsumirMensagemCommand request, CancellationToken ct)
    {
        await _rabbit.ConsumirAsync<dynamic>(request.Fila, async msg =>
        {
            // aqui vocÃª processa a mensagem (pode logar, salvar, etc.)
            await Task.CompletedTask;
        }, ct);

        return ApiResponse<object>.Ok(new { request.Fila }, "Consumidor iniciado");
    }
}
