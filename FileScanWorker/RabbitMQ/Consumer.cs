namespace FileScanWorker.RabbitMQ;

public class Consumer(
    ProcessMessageService processMessageService,
    RabbitMqConsumerSettings settings,
    ILogger<Consumer> logger
) : IAsyncDisposable
{
    public bool IsConnected => _connection?.IsOpen == true 
        && _channel?.IsOpen == true;
    
    private static readonly ushort ProcessorCount = (ushort)Environment.ProcessorCount;
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _topologyIsDeclared;


    private readonly ConnectionFactory factory = new()
    {
        HostName = settings.HostName,
        ConsumerDispatchConcurrency = ProcessorCount,
        AutomaticRecoveryEnabled = true,
    };
    
    public async Task StartAsync(CancellationToken token)
    {
        logger.LogInformation("Consumer is starting ...");
        
            try
            {
                await SubscribeAsync(token);
                logger.LogInformation("... consumer started");
            }
            catch (Exception ex)
            {
                logger.LogError("La connexion à RabbitMQ a échouée");
                // logger.LogError(ex); ACTIVER EN PRODUCTION
            }
    }

    private async Task SubscribeAsync(CancellationToken token)
    {
        _connection = await factory.CreateConnectionAsync(token);

        _channel = await _connection.CreateChannelAsync(null, token);

        if (!_topologyIsDeclared)
        {
            await DeclareTopologyAsync(_channel);

            _topologyIsDeclared = true;
        }

        await CreateConsumerAsync(_channel);
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        logger.LogInformation("Le message est en cours de traitement ...");

        var innerChannel = ((AsyncEventingBasicConsumer)sender).Channel;
        var message = GetMessageFromBody<DocumentMessage>(ea.Body);
        logger.LogInformation($"message reçu: {message}");

        try
        {
            logger.LogInformation("Start Scan processing ...");
            await processMessageService.ProcessAsync(message);
            logger.LogInformation("... Finish Scan processing");

            await innerChannel.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch (Exception e)
        {
            logger.LogError($"Problème avec le scan {e.Message}, envoyer le message dans l'ErrorQueue");

            // SORTIR LE MESSAGE DE LA QUEUE PRINCIPALE : ÉVITER LES BOUCLES INFINIES
            await innerChannel.BasicNackAsync(
                deliveryTag: ea.DeliveryTag,
                multiple: false,
                requeue: false
            );
        }
    }

    private async Task CreateConsumerAsync(IChannel c)
    {
        var consumer = new AsyncEventingBasicConsumer(c);

        consumer.ReceivedAsync += HandleMessageAsync;

        await c.BasicConsumeAsync(
            queue: settings.Queue,
            autoAck: false,
            consumer: consumer
        );
    }

    private async Task DeclareTopologyAsync(IChannel c)
    {
        await c.ExchangeDeclareAsync(
            exchange: settings.Exchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );

        await c.QueueDeclareAsync(
            queue: settings.DeadLetterQueue,
            durable: true,
            autoDelete: false,
            exclusive: false
        );

        var mainQueueArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", settings.Exchange },
            { "x-dead-letter-routing-key", settings.DeadLetterQueue }
        };

        await c.QueueDeclareAsync(
            queue: settings.Queue,
            durable: true,
            autoDelete: false,
            exclusive: false,
            arguments: mainQueueArgs
        );

        await c.QueueBindAsync(
            queue: settings.Queue,
            exchange: settings.Exchange,
            routingKey: settings.Queue
        );

        await c.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: ProcessorCount,
            global: false
        );
    }

    private T GetMessageFromBody<T>(ReadOnlyMemory<byte> body) where T : class
    {
        var bodyArray = body.ToArray();
        var bodyStr = Encoding.UTF8.GetString(bodyArray);

        return JsonSerializer.Deserialize<T>(bodyStr) ?? throw new Exception("Le message est null");
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is { IsOpen: true })
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }

        if (_connection is { IsOpen: true })
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        logger.LogInformation("Producer properly disposed");
    }
}