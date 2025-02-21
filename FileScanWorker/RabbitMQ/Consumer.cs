using System.Text;
using System.Text.Json;
using Domain.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FileScanWorker.RabbitMQ;

public class Consumer<T>(IConnection connection, IChannel channel) : IAsyncDisposable
{
    public async Task SubscribeAsync() {
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, ea) => {
            var body = ea.Body.ToArray();

            var messageStr = Encoding.UTF8.GetString(body);

            var message = JsonSerializer.Deserialize<T>(messageStr);

            Console.WriteLine(message);

            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        };

        await channel.BasicConsumeAsync(
            queue: "document-to-scan",
            autoAck: false,
            consumer: consumer
        );
    }

    public async ValueTask DisposeAsync() {
        await channel.CloseAsync();
        await channel.DisposeAsync();
        
        await connection.CloseAsync();
        await connection.DisposeAsync();
        Console.WriteLine("Consumer properly disposed");
    }
}