using System.Text;
using System.Text.Json;
using Domain.Contracts;
using FileScanWorker.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FileScanWorker.RabbitMQ;

public class Consumer(FileScanService scanService, SignalRClientService signalrService) : IAsyncDisposable
{
    private readonly ushort processorCount = (ushort)Environment.ProcessorCount;
    private IConnection? _connection;
    private IChannel? _channel;

    public async Task StartAsync() {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            ConsumerDispatchConcurrency = processorCount
        };
        
        _connection = await factory.CreateConnectionAsync();
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

        Console.WriteLine("Start Scan processing ...");
        var document = await scanService.DocumentScanProcess(message);
        Console.WriteLine("... Finish Scan processing");
        if (document is null) return;
        
        Console.WriteLine("Start SignalR Notify Statut Updated ...");
        var notification = new DocumentStatutUpdatedMessage
        {
            DemandeAvisId = document.DemandeAvisId,
            DocumentId = document.Id,
            DocumentStatut = document.StatutCode,
            DocumentType = document.Type
        };
        await signalrService.SendStatutUpdated(notification);
        Console.WriteLine("... Finish SignalR Notify");

        await _channel!.BasicAckAsync(ea.DeliveryTag, false);
    }

    public async ValueTask DisposeAsync() {

        Console.WriteLine("ConsumerService will be disposing ...");
        if (_channel != null) await _channel.CloseAsync();
        if (_connection != null) await _connection.CloseAsync();
        Console.WriteLine("... ConsumerService is disposed.");
    }
}