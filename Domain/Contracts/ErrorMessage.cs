namespace Domain.Contracts;

public record ErrorMessage(string ErrorDescription,  string errorAt, object MessageSended);