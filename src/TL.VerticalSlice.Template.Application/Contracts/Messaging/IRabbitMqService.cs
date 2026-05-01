namespace TL.VerticalSlice.Template.Application.Contracts.Messaging;

public interface IRabbitMqService
{
    Task ProduzirAsync<T>(string fila, T mensagem);
    Task ConsumirAsync<T>(string fila, Func<T, Task> callback, CancellationToken cancellationToken);
}
