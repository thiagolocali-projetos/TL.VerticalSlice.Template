using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TL.VerticalSlice.Template.Application.Contracts.Messaging;

namespace TL.VerticalSlice.Template.Infrastructure.Messaging;

public class RabbitMqService : IRabbitMqService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMqService> _logger;

    public RabbitMqService(ILogger<RabbitMqService> logger)
    {
        _logger = logger;
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        _connection = Task.Run(() => factory.CreateConnectionAsync(CancellationToken.None)).Result;
        _channel = Task.Run(() => _connection.CreateChannelAsync()).Result;
        _logger.LogInformation("ðŸ° RabbitMQ conectado");
    }

    public async Task ProduzirAsync<T>(string fila, T mensagem)
    {
        await _channel.QueueDeclareAsync(fila, true, false, false, null);
        var json = JsonSerializer.Serialize(mensagem);
        var body = Encoding.UTF8.GetBytes(json);
        await _channel.BasicPublishAsync("", fila, false, new BasicProperties(), body);
        _logger.LogInformation("ðŸ“¤ Publicado em {Fila}: {Mensagem}", fila, json);
    }

    public async Task ConsumirAsync<T>(string fila, Func<T, Task> callback, CancellationToken ct)
    {
        await _channel.QueueDeclareAsync(fila, true, false, false, null);
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var obj = JsonSerializer.Deserialize<T>(json);
                if (obj is not null) await callback(obj);
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "âŒ Erro ao processar mensagem");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };
        await _channel.BasicConsumeAsync(fila, false, consumer, ct);
        _logger.LogInformation("ðŸ‘‚ Escutando {Fila}", fila);
    }

    public void Dispose()
    {
        _channel?.CloseAsync();
        _connection?.CloseAsync();
    }
}
