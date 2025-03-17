using System.Net.Http.Json;
using Domain.Contracts;
using Domain.Entities;

namespace FileScanWorker.Services;

public class FileScanService(IHttpClientFactory factory)
{
    public async Task<Document?> DocumentScanProcess(DocumentToScanMessage message) {
        Console.WriteLine("Démarer le processus SCAN FILE");

        Document? document;

        try {
            document = await GetDocument(message.DocumentId);
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

        try {
            await PutStatutAndAddress(isValid, document);
            Console.WriteLine("Mise à jour de la DB");
            Console.WriteLine("Fin du processus SCAN FILE");
        }
        catch (Exception e) {
            Console.WriteLine($"Erreur PUT de mise à jour de la DB: {e.Message}");
            throw;
        }
        

        document.StatutCode = isValid ? "Valide" : "Corrompu";
        document.Type = new Random().Next(0, 5) switch
        {
            0 => "Projet",
            1 => "Mandat",
            2 => "Annexe",
            3 => "AutreDocument",
            4 => "Confirmation"
        };

        return document;
    }

    private async Task<Document?> GetDocument(int documentId) {

        var httpClient = factory.CreateClient("api");
        await Task.Delay(600);

        return await httpClient.GetFromJsonAsync<Document>($"/document/{documentId}");
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

    private async Task PutStatutAndAddress(bool isValid, Document document) {
        
        var statut = isValid ? "Valide" : "Corrompu";

        document.StatutCode = statut;
        document.Addresse = statut == "Valide" 
            ? "address définitive dans EPROLEX_FILE" 
            : "Pas d'adresse";
        
        var httpClient = factory.CreateClient("api");
        await Task.Delay(600);

        await httpClient.PutAsJsonAsync<Document>($"/document", document);
    }

    
}