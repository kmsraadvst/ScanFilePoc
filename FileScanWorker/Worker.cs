using Domain.Contracts;
using FileScanWorker.RabbitMQ;
using FileScanWorker.Services;

namespace FileScanWorker;

public class Worker(ILogger<Worker> logger, Consumer consumer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {

        await consumer.StartAsync();
        
        while (!stoppingToken.IsCancellationRequested) {
            
            // logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}