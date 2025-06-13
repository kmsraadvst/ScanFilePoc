
namespace BlazorServerUI.RabbitMQ;

public class Producer<TMessage>
    where TMessage : class
{
    private readonly ILogger<Producer<TMessage>> _logger;

    private readonly ConnectionFactory _factory;

    private IConnection? _connection;
    private IChannel? _channel;

    private readonly SemaphoreSlim semaphore = new(1);
    private bool _topologyIsDeclared;

    private readonly string _exchangeName;
    private readonly string _queueName;

    public Producer(ILogger<Producer<TMessage>> logger, RabbitMqProducerSettings settings)
    {
        _logger = logger;

        _factory = new()
        {
            HostName = settings.HostName,
            AutomaticRecoveryEnabled = true,
        };


        var workerConfiguration = settings
            .WorkerConfigurations
            .First(wc => wc.TypeMessage == typeof(TMessage).Name);

        _exchangeName = workerConfiguration.Exchange;
        _queueName = workerConfiguration.Queue;
    }


    public async Task EnsureIsStartedAsync()
    {
        await semaphore.WaitAsync();

        try
        {
            if (_connection is null || !_connection.IsOpen)
            {
                _connection = await _factory.CreateConnectionAsync();
            }

            if (_channel is null || !_channel.IsOpen)
            {
                _channel = await _connection.CreateChannelAsync();
            }

            if (!_topologyIsDeclared)
            {
                await DeclareTopologyAsync(_channel);

                _topologyIsDeclared = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ exception lors de la connexion ou de l'initialisation");
            throw new RabbitMqUnavailableException("Le serveur RabbitMQ est temporairement indisponible", ex);
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

        if (_channel is not null)
        {
            await _channel.BasicPublishAsync(
                exchange: _exchangeName,
                routingKey: _queueName,
                mandatory: true,
                basicProperties: properties,
                body: body
            );
        }
    }

    private async Task DeclareTopologyAsync(IChannel c)
    {
        // DECLARATION DE L'EXCHANGE
        await c.ExchangeDeclareAsync(
            exchange: _exchangeName,
            type: ExchangeType.Direct,
            durable: true
        );
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

        _logger.LogDebug("Producer properly disposed");
    }
}