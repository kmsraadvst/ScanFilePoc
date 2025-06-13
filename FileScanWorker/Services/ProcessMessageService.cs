using Domain.Contracts.Messages;
using Domain.Contracts.Notifications;
using Domain.Enums;

namespace FileScanWorker.Services;

public class ProcessMessageService(
    DocumentRepository repo,
    CheckExtensionService checkExtensionService,
    FileScanService fileScanService,
    SignalRClientService<DocumentUpdatedNotification> signalRService)
{
    public async Task ProcessAsync(DocumentMessage message)
    {
        Console.WriteLine("Début du Processing du message ...");

        // TEST DLQ 
        if (new Random().Next(1, 5) == 1) throw new Exception("BOUM FOR Dead Letter Queue");
        
        
        // 1. RÉCUPÉRER LE DOCUMENT EN DB
        Console.WriteLine($"Allez chercher le document [{message.DocumentId}] en DB");
        var document = await repo.GetByIdAsync(message.DocumentId);
        Console.WriteLine($"Récupération en DB du Document Id[{document.Id}]");


        // 2. VÉRIFIER L'EXTENSION
        Console.WriteLine("Vérifier l'extension");
        var isExtensionValid = await checkExtensionService.CheckAsync(document);
        Console.WriteLine($"Extension est valide: {isExtensionValid}");


        // 3. SCANNER LES MALWARES
        Console.WriteLine("Scanner les malwares");
        var isScanValid = await fileScanService.ScanFakeAsync();
        Console.WriteLine($"Le fichier est safe: {isScanValid}");

        
        // 4. SUPPRIMER LE FICHIER OU LE DÉPLACER DANS LE DOSSIER SAFE
        // TODO MoveFileService


        // 5. METTRE À JOUR LE DOCUMENT EN DB
        var isValid = isExtensionValid && isScanValid;

        Console.WriteLine($"Document [{document.Id}] est valide ? {isValid}");
        document.StatutCode = isValid ? StatutDocument.Valide : StatutDocument.Corrompu;

        Console.WriteLine($"Mise à jour Document [{document.Id}] en DB");
        await repo.UpdatAsync(document);


        // 6. NOTIFIER LE NOUVEAU STATUT VIA SIGNALR
        Console.WriteLine("Notifier le changement de statut du document par SignalR");
        var notification = new DocumentUpdatedNotification(
            DemandeAvisId: document.DemandeAvisId,
            DocumentId: document.Id,
            TypeDocument: document.TypeCode,
            DocumentStatut: document.StatutCode
        );

        await signalRService.NotifyAsync(HubMethods.DocumentStatutUpdated, notification);


        Console.WriteLine("... fin du Processing #############\n");
    }
}