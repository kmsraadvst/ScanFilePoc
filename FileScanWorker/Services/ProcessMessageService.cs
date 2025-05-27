using Domain.Enums;

namespace FileScanWorker.Services;

public class ProcessMessageService(
    DocumentRepository repo,
    CheckExtensionService checkExtensionService,
    FileScanService fileScanService,
    SignalRClientService signalRService)
{
    public async Task ProcessAsync(DocumentToScanMessage message)
    {
        Console.WriteLine("Début du Processing du message ...");
        
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

        
        // 4. METTRE À JOUR LE DOCUMENT EN DB
        var isValid = isExtensionValid && isScanValid;
        
        Console.WriteLine($"Document [{document.Id}] est valide ? {isValid}");
        document.StatutCode = isValid ? StatutDocument.Valide : StatutDocument.Corrompu;

        Console.WriteLine($"Mise à jour Document [{document.Id}] en DB");
        await repo.UpdatAsync(document);

        
        // 5. NOTIFIER LE NOUVEAU STATUT VIA SIGNALR
        Console.WriteLine("Notifier le chnagement de statut du document par SignalR");
        await signalRService.SendDocumentUpdated(document);
        
        

        Console.WriteLine("... fin du Processing #############\n");
    }
}