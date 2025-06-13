using System.Diagnostics;

namespace FileScanWorker;

public class Worker(ILogger<Worker> logger, Consumer consumer) : BackgroundService
{
    private bool _isReconnecting;
    private Stopwatch sw = new();
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {

        logger.LogInformation("Worker is started ...");
        await consumer.StartAsync(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested) {
            
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            if (consumer.IsConnected)
            {
                logger.LogInformation("Le consumer est connecté");

                if (_isReconnecting)
                {
                    sw.Stop();
                    _isReconnecting = false;
                    logger.LogInformation($"RabbitMQS a été indisponible pendant {sw.Elapsed.TotalSeconds} secondes");
                    sw.Reset();
                }
            }
            else
            {
                logger.LogInformation("Le consumer n'est plus connecter, tentative de reconnection ...");

                if (!_isReconnecting)
                {
                    _isReconnecting = true;
                    sw.Start();
                }
                
                
                await consumer.StartAsync(stoppingToken);
                
                var messageConnection = consumer.IsConnected
                    ? "... consumer reconnecté"
                    : "... la tentative a échoué";
                
                logger.LogInformation(messageConnection);
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
        
        logger.LogInformation("... worker is stopped");
    }
}