namespace FileScanWorker.RabbitMQ;

public class Consumer(ProcessMessageService processMessageService) : IAsyncDisposable
{
    private readonly ushort processorCount = (ushort)Environment.ProcessorCount;
    private IConnection? _connection;
    private IChannel? _channel;

    public async Task StartAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            ConsumerDispatchConcurrency = processorCount,
            AutomaticRecoveryEnabled = true,
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
            queue: "error-scanfile",
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
        Console.WriteLine("Le message est en cours de traitement ...");

        var innerChannel = ((AsyncEventingBasicConsumer)sender).Channel;
        var message = GetMessageFromBody<DocumentToScanMessage>(ea.Body);
        Console.WriteLine($"message reçu: {message}");

        try
        {
            Console.WriteLine("Start Scan processing ...");
            await processMessageService.ProcessAsync(message);
            Console.WriteLine("... Finish Scan processing");

            await innerChannel.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Problème avec le scan {e.Message}, envoyer le message dans l'ErrorQueue");
            
            // ENVOYER UN MESSAGE DANS L'ERROR_QUEUE
            await PublishErrorMessageAsync(e.Message, innerChannel, message);
            
            // SORTIR LE MESSAGE DE LA QUEUE PRINCIPALE : ÉVITER LES BOUCLES INFINIES
            await innerChannel.BasicAckAsync(ea.DeliveryTag, false);

        }
    }

    private T GetMessageFromBody<T>(ReadOnlyMemory<byte> body) where T : class
    {
        var bodyArray = body.ToArray();
        var bodyStr = Encoding.UTF8.GetString(bodyArray);

        return JsonSerializer.Deserialize<T>(bodyStr) ?? throw new Exception("Le message est null");
    }

    private async Task PublishErrorMessageAsync(string exceptionMessage, IChannel channel, DocumentToScanMessage message)
    {
        var errorMessage = new ErrorMessage(
            $"Problème avec le scan {exceptionMessage}, envoyer le message dans l'ErrorQueue",
            $"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}",
            message
        );

        var errorMessageStr = JsonSerializer.Serialize(errorMessage);

        var errorBody = Encoding.UTF8.GetBytes(errorMessageStr);
        await channel.BasicPublishAsync("", "error-scanfile", errorBody);
    }

    public async ValueTask DisposeAsync()
    {
        Console.WriteLine("ConsumerService will be disposing ...");
        if (_channel != null) await _channel.CloseAsync();
        if (_connection != null) await _connection.CloseAsync();
        Console.WriteLine("... ConsumerService is disposed.");
    }
}