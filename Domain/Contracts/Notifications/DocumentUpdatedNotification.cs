namespace Domain.Contracts.Notifications;

public record DocumentUpdatedNotification
(
    int DemandeAvisId,
    int DocumentId,
    string TypeDocument,
    string DocumentStatut
);