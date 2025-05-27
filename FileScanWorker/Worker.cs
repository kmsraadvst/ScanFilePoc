namespace FileScanWorker;

public class Worker(ILogger<Worker> logger, Consumer consumer, SignalRClientService signalR) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {

        await consumer.StartAsync();
        await signalR.StartSignalRConnection();
        
        while (!stoppingToken.IsCancellationRequested) {
            
            // logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}