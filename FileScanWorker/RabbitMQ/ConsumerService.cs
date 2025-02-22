using System.Text;
using System.Text.Json;
using Domain.Contracts;
using FileScanWorker.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FileScanWorker.RabbitMQ;

public class ConsumerService : IAsyncDisposable
{
    private readonly ushort processorCount = (ushort)Environment.ProcessorCount;
    private readonly IConnectionFactory _factory;
    private IConnection? _connection;
    private IChannel? _channel;

    private readonly FileScanService _service;

    public ConsumerService(FileScanService service) {
        _factory = new ConnectionFactory
        {
            HostName = "localhost",
            ConsumerDispatchConcurrency = processorCount
        };

        _service = service;
    }

    public async Task StartAsync() {
        _connection = await _factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(
            queue: "document-to-scan",
            durable: true,
            autoDelete: false,
            exclusive: false
        );

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: processorCount,
            global: false
        );

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += HandleMessage;

        await _channel.BasicConsumeAsync(
            queue: "document-to-scan",
            autoAck: false,
            consumer: consumer
        );
    }

    private async Task HandleMessage(object sender, BasicDeliverEventArgs ea) {
        Console.WriteLine("Message is handling");

        var body = ea.Body.ToArray();
        var messageStr = Encoding.UTF8.GetString(body);
        var message = JsonSerializer.Deserialize<DocumentToScanMessage>(messageStr);

        if (message is null) {
            // HANDLE ERROR

            await _channel!.BasicAckAsync(ea.DeliveryTag, false);
            return;
        }

        Console.WriteLine(message);

        Console.WriteLine("Start processing ...");
        await _service.DocumentScanProcess(message);
        Console.WriteLine("... Finish processing");

        await _channel!.BasicAckAsync(ea.DeliveryTag, false);
    }

    public async ValueTask DisposeAsync() {

        Console.WriteLine("ConsumerService will be disposing ...");
        if (_channel != null) await _channel.CloseAsync();
        if (_connection != null) await _connection.CloseAsync();
        Console.WriteLine("... ConsumerService is disposed.");
    }
}