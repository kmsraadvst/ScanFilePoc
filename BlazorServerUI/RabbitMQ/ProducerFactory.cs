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
            queue: "document-to-scan",
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        return new Producer<T>(connection, channel);
    }
}