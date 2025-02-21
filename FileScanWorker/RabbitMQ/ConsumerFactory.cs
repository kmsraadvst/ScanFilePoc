using Domain.Contracts;
using RabbitMQ.Client;

namespace FileScanWorker.RabbitMQ;

public static class ConsumerFactory
{
    private static readonly ushort ProcessorCount = (ushort)Environment.ProcessorCount;

    public static async Task<Consumer<T>> CreateConsumerAsync<T>() {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            ConsumerDispatchConcurrency = ProcessorCount
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "document-to-scan",
            durable: true,
            autoDelete: false,
            exclusive: false
        );

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: ProcessorCount,
            global: false
        );

        return new Consumer<T>(connection, channel);
    }
}