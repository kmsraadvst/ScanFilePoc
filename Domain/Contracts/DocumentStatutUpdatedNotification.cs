namespace Domain.Contracts;

public record DocumentStatutUpdatedNotification
(
    int DemandeAvisId,
    int DocumentId,
    string TypeDocument,
    string DocumentStatut
);