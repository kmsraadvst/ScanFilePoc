namespace FileScanWorker.Services;

public class FileScanService(DocumentRepository repo)
{
    public async Task<Document?> DocumentScanProcess(DocumentToScanMessage message) {
        Console.WriteLine("Démarer le processus SCAN FILE");

        Document? document;

        try {
            document = await repo.GetByIdAsync(message.DocumentId);
            if (document is null)
            {
                Console.WriteLine($"Le document Id[{message.DocumentId}] n'existe pas");

                return null;
            }
            Console.WriteLine($"Worker reçoit de l'API Document Id[{document.Id}]");
        }
        catch (Exception e) {
            Console.WriteLine($"Erreur GET pour chercher le document: {e.Message}");
            throw;
        }
       
        
        var path = await CalculatePath(document);
        Console.WriteLine("chemin calculé");

        var isValidExtension = await CheckExtension(path);
        Console.WriteLine($"Extension vérifiée isValidExtension={isValidExtension}");
        
        var isValidScan = await CheckScanFile(path);
        Console.WriteLine($"Scan (Antivirus) vérifié isValidScan={isValidScan}");
        
        var isValid = isValidExtension && isValidScan;
        Console.WriteLine($"le document est-il valide ? : {isValid}");

        if (isValid) {
            await MoveToEprolex_File(document);
            Console.WriteLine("Document valide déplacé dans le File System définitif [EPROLEX_FILE]");
        }
        
        document.StatutCode = isValid ? "Valide" : "Corrompu";
        
        try {
            await repo.UpdateStatutAndAddressAsync(document);
            Console.WriteLine("Mise à jour de la DB");
            Console.WriteLine("Fin du processus SCAN FILE");
        }
        catch (Exception e) {
            Console.WriteLine($"Erreur PUT de mise à jour de la DB: {e.Message}");
            throw;
        }
        

        

        return document;
    }

    

    private async Task<string> CalculatePath(Document document) {
        
        await Task.Delay(600);

        return "path";
    }

    private async Task<bool> CheckScanFile(string filePath) {

        
        for (var i = 0; i < 6; i++) {
            Console.Write(". ");
            await Task.Delay(100);
        }
        Console.WriteLine(". ");
        
        // SCAN FILE

        return new Random().Next(1, 9) != 5;
    }
    
    private async Task<bool> CheckExtension(string filePath) {
        
        await Task.Delay(600);
        
        // CHECK EXTENSION

        return new Random().Next(1, 9) != 5;
    }
    
    private async Task MoveToEprolex_File(Document document) {
        // MOVE TO EPROLEX_FILE

        await Task.Delay(600);
    }

    

    
}