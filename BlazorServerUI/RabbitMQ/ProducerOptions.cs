namespace BlazorServerUI.RabbitMQ;

public record ProducerOptions(
    string TypeMessage,
    string ExchangeName,
    string QueueName,
    string ErrorQueueName
);

public class ProducersOptions : List<ProducerOptions>
{
}