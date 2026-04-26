namespace TL.Exemplo.Application.Contracts.Messaging;

public interface IKafkaService
{
    Task ProduzirAsync<T>(string topico, string key, T mensagem, CancellationToken ct = default);
    Task ConsumirAsync<T>(string topico, Func<T, Task> callback, CancellationToken ct = default);
}