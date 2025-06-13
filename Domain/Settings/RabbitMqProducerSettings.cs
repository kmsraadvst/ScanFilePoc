namespace Domain.Settings;

public class RabbitMqProducerSettings
{
    public string HostName { get; set; } = string.Empty;
    public List<WorkerConfiguration> WorkerConfigurations { get; set; } = [];
}