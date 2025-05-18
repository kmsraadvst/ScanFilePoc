namespace Domain.Exceptions;

public class GetDocumentException(string message, Exception? innerException = null) 
    : Exception(message, innerException) { }