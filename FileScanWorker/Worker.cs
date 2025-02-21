using Domain.Contracts;
using FileScanWorker.RabbitMQ;

namespace FileScanWorker;

public class Worker(ILogger<Worker> logger) : BackgroundService
{


    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {

        await using var consumer = await ConsumerFactory.CreateConsumerAsync<DocumentToScanMessage>();

        await consumer.SubscribeAsync();
        
        while (!stoppingToken.IsCancellationRequested) {
            
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            

            await Task.Delay(3000, stoppingToken);
        }
    }
}