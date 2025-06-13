namespace Domain.Settings;

public class WorkerConfiguration
{
    public string TypeMessage { get; set; } = string.Empty;
    public string Queue { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
}