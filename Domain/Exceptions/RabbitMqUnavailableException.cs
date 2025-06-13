namespace Domain.Exceptions;

public class RabbitMqUnavailableException(string message, Exception ex) : Exception(message, ex) { }