namespace BlazorServerUI.RabbitMQ;

public class Producer<T>(IConnection connection, IChannel channel) : IAsyncDisposable
{
    public async Task PublishAsync(T message) {
        
        var messageStr = JsonSerializer.Serialize(message);

        var body = Encoding.UTF8.GetBytes(messageStr);

        // Conserve le message en cas d'extinction du serveur RabbitMQ
        var properties = new BasicProperties
        {
            Persistent = true,
        };

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "document-to-scan",
            mandatory: true,
            basicProperties: properties,
            body: body
        );
    }

    public async ValueTask DisposeAsync() {
        await channel.CloseAsync();
        await channel.DisposeAsync();
        
        await connection.CloseAsync();
        await connection.DisposeAsync();
        Console.WriteLine("Producer properly disposed");

    }
}