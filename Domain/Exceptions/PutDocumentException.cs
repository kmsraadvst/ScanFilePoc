namespace Domain.Exceptions;

public class PutDocumentException(string message, Exception? innerException = null) 
    : Exception(message, innerException) { }
