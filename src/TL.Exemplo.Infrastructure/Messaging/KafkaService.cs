using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TL.Exemplo.Application.Contracts.Messaging;

namespace TL.Exemplo.Infrastructure.Messaging;

public class KafkaService : IKafkaService, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaService> _logger;

    public KafkaService(ILogger<KafkaService> logger)
    {
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
        _logger.LogInformation("📡 Kafka conectado em localhost:9092");
    }

    public async Task ProduzirAsync<T>(string topico, string key, T mensagem, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(mensagem);
        var kafkaMessage = new Message<string, string>
        {
            Key = key,
            Value = json
        };

        var result = await _producer.ProduceAsync(topico, kafkaMessage, ct);
        _logger.LogInformation(
            "📤 Kafka produzido: Tópico={Topico}, Key={Key}, Partition={Partition}, Offset={Offset}",
            result.Topic, result.Key, result.Partition, result.Offset);
    }

    public Task ConsumirAsync<T>(string topico, Func<T, Task> callback, CancellationToken ct = default)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "tl-exemplo-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(topico);

        // Executa em background (fire-and-forget para estudos)
        _ = Task.Run(async () =>
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var result = consumer.Consume(ct);
                    var obj = JsonSerializer.Deserialize<T>(result.Message.Value);

                    if (obj is not null)
                    {
                        _logger.LogInformation(
                            "📥 Kafka consumido: Tópico={Topico}, Key={Key}, Offset={Offset}",
                            result.Topic, result.Message.Key, result.Offset);
                        await callback(obj);
                    }

                    consumer.Commit(result);
                }
            }
            catch (OperationCanceledException) { }
            finally { consumer.Close(); }
        }, ct);

        _logger.LogInformation("👂 Kafka escutando tópico: {Topico}", topico);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _producer?.Dispose();
        _logger.LogInformation("📡 Kafka desconectado");
    }
}