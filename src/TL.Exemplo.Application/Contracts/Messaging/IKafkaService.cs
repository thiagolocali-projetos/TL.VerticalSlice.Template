namespace TL.Exemplo.Application.Contracts.Messaging;

public interface IRabbitMqService
{
    Task PublicarAsync<T>(string fila, T mensagem);
    Task ConsumirAsync<T>(string fila, Func<T, Task> callback, CancellationToken cancellationToken);
}