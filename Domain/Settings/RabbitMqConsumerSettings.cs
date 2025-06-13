namespace Domain.Settings;

public class RabbitMqConsumerSettings
{
    public string HostName { get; set; } = string.Empty;
    public string TypeMessage { get; set; } = string.Empty;
    public string Queue { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public string DeadLetterQueue { get; set; } = string.Empty;
}