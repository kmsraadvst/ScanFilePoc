namespace Domain.Exceptions;

public class ScanFileApiException(string message, Exception? innerException = null) 
    : Exception(message, innerException) { }