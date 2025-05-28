namespace BlazorServerUI.RabbitMQ;

public static class ProducerFactory
{

    public static async Task<Producer<T>> CreateProducerAsync<T>() {

        var factory = new ConnectionFactory
        {
            HostName = "localhost",
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "dlq-scanfile",
            durable: true,
            autoDelete: false,
            exclusive: false
        );
        
        var mainQueueArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", "" }, 
            { "x-dead-letter-routing-key", "dlq-scanfile" }
        };
        
        await channel.QueueDeclareAsync(
            queue: "document-to-scan",
            durable: true,
            autoDelete: false,
            exclusive: false,
            arguments: mainQueueArgs
        );

        return new Producer<T>(connection, channel);
    }
}