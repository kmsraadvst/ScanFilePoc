using Microsoft.Extensions.Options;

namespace BlazorServerUI.RabbitMQ;

public class Producer<TMessage> where TMessage : class
{
    private readonly ConnectionFactory factory = new()
    {
        HostName = "localhost",
    };

    private IConnection? connection;
    private IChannel? channel;

    private readonly SemaphoreSlim semaphore = new(1);
    private bool queueIsDeclared;
    
    private readonly string exchangeName;
    private readonly string queueName;
    private readonly string errorQueueName;
    private readonly ILogger<Producer<TMessage>> _logger;
    public Producer(ILogger<Producer<TMessage>> logger, IOptions<ProducersOptions> options)
    {
        _logger = logger;

        var option = options.Value.First(option => option.TypeMessage == typeof(TMessage).Name);
        exchangeName = option.ExchangeName;
        queueName = option.QueueName;
        errorQueueName = option.ErrorQueueName;
    }


    public async Task EnsureIsStartedAsync()
    {
        await semaphore.WaitAsync();

        try
        {
            if (connection is null || !connection.IsOpen)
            {
                connection = await factory.CreateConnectionAsync();
            }

            if (channel is null || !channel.IsOpen)
            {
                channel = await connection.CreateChannelAsync();
            }
            
            if (!queueIsDeclared)
            {
                await DeclareTopology(channel);

                queueIsDeclared = true;
            }
        }
        finally
        {
            semaphore.Release();
        }
    }
    
    public async Task PublishAsync(TMessage message)
    {
        await EnsureIsStartedAsync();
        
        var messageStr = JsonSerializer.Serialize(message);

        var body = Encoding.UTF8.GetBytes(messageStr);

        // Conserve le message en cas d'extinction du serveur RabbitMQ
        var properties = new BasicProperties
        {
            Persistent = true,
        };

        if (channel is not null)
        {
            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: queueName,
                mandatory: true,
                basicProperties: properties,
                body: body
            );
        }
    }

    private async Task DeclareTopology(IChannel c)
    {
        // DECLARATION DE L'EXCHANGE
        await c.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Direct,
            durable: true
        );
        
        // DECLARATION DES QUEUES
        await c.QueueDeclareAsync(
            queue: errorQueueName,
            durable: true,
            autoDelete: false,
            exclusive: false
        );

        var mainQueueArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", exchangeName },
            { "x-dead-letter-routing-key", errorQueueName }
        };

        await c.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            autoDelete: false,
            exclusive: false,
            arguments: mainQueueArgs
        );
        
        // DECLARATION DES BINDINGS
        await c.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: queueName
        );
        
        await c.QueueBindAsync(
            queue: errorQueueName,
            exchange: exchangeName,
            routingKey: errorQueueName
        );

    }

    public async ValueTask DisposeAsync()
    {
        if (channel is not null)
        {
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        if (connection is not null)
        {
            await connection.CloseAsync();
            await connection.DisposeAsync();
        }

        _logger.LogDebug("Producer properly disposed");
    }
}