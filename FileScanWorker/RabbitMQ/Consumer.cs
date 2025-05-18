using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FileScanWorker.RabbitMQ;

public class Consumer(
    FileScanService scanService, 
    SignalRClientService signalrService,
    DocumentRepository repository) : IAsyncDisposable
{
    private readonly ushort processorCount = (ushort)Environment.ProcessorCount;
    private IConnection? _connection;
    private IChannel? _channel;

    public async Task StartAsync()
    {
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

        await _channel.QueueDeclareAsync(
            queue: "error",
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

    private async Task HandleMessage(object sender, BasicDeliverEventArgs ea)
    {
        Console.WriteLine("Message is handling");

        var innerChannel = ((AsyncEventingBasicConsumer)sender).Channel;

        var body = ea.Body.ToArray();
        var messageStr = Encoding.UTF8.GetString(body);
        var message = JsonSerializer.Deserialize<DocumentToScanMessage>(messageStr);
        
        if (message is null)
        {
            // HANDLE ERROR
            Console.WriteLine("Le message est null");
            await innerChannel.BasicAckAsync(ea.DeliveryTag, false);
            return;
        }

        Console.WriteLine(message);

        try
        {
            Console.WriteLine("Start Scan processing ...");
            var document = await scanService.DocumentScanProcess(message);
            Console.WriteLine("... Finish Scan processing");

            Console.WriteLine("Start SignalR Notify Statut Updated ...");
            var notification = new DocumentStatutUpdatedNotification
            (
                DemandeAvisId: document.DemandeAvisId,
                DocumentId: document.Id,
                DocumentStatut: document.StatutCode,
                TypeDocument: document.TypeCode
            );
            await signalrService.SendStatutUpdated(notification);
            Console.WriteLine("... Finish SignalR Notify");

            await innerChannel.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Problème avec le scan {e.Message}, envoyer le message dans l'ErrorQueue");
            // ENVOYER UN MESSAGE DANS L'ERROR_QUEUE
            var errorMessage = new ErrorMessage(
                $"Problème avec le scan {e.Message}, envoyer le message dans l'ErrorQueue",
                DateTime.Now.ToLongDateString(),
                message
            );

            var errorMessageStr = JsonSerializer.Serialize(errorMessage);

            var errorBody = Encoding.UTF8.GetBytes(errorMessageStr);
            await innerChannel.BasicPublishAsync("", "error", errorBody);
            
            // SORTIR LE MESSAGE DE LA QUEUE PRINCIPALE : ÉVITER LES BOUCLES INFINIES
            await innerChannel.BasicAckAsync(ea.DeliveryTag, false);

        }
    }

    public async ValueTask DisposeAsync()
    {
        Console.WriteLine("ConsumerService will be disposing ...");
        if (_channel != null) await _channel.CloseAsync();
        if (_connection != null) await _connection.CloseAsync();
        Console.WriteLine("... ConsumerService is disposed.");
    }
}