namespace Domain.Contracts;

public class DocumentStatutUpdatedMessage
{
    public int DemandeAvisId { get; set; }
    public Guid DocumentId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentStatut { get; set; } = string.Empty;
}