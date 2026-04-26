using MediatR;
using TL.Exemplo.Application.Common.Models;
using TL.Exemplo.Application.Contracts.Messaging;

namespace TL.Exemplo.Application.Features.Rabbit.ConsumirMensagem;

public record ConsumirMensagemCommand(string Fila) : IRequest<ApiResponse<object>>;

public class ConsumirMensagemHandler : IRequestHandler<ConsumirMensagemCommand, ApiResponse<object>>
{
    private readonly IRabbitMqService _rabbit;
    public ConsumirMensagemHandler(IRabbitMqService rabbit) => _rabbit = rabbit;

    public async Task<ApiResponse<object>> Handle(ConsumirMensagemCommand request, CancellationToken ct)
    {
        await _rabbit.ConsumirAsync<dynamic>(request.Fila, async msg =>
        {
            // aqui você processa a mensagem (pode logar, salvar, etc.)
            await Task.CompletedTask;
        }, ct);

        return ApiResponse<object>.Ok(new { request.Fila }, "Consumidor iniciado");
    }
}