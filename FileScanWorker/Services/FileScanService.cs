using System.Net.Http.Json;
using Domain.Contracts;
using Domain.Entities;

namespace FileScanWorker.Services;

public class FileScanService(IHttpClientFactory factory)
{
    public async Task DocumentScanProcess(DocumentToScanMessage message) {
        Console.WriteLine("Start process SCAN FILE");
        
        var document = await GetDocument(message.DocumentId);
        Console.WriteLine($"Worker receive from API Document Id[{document?.Id}]");
        
        var path = await CalculatePath(document!);
        Console.WriteLine("path calculated");

        var isValidExtension = await CheckExtension(path);
        Console.WriteLine($"Extension Checked: {isValidExtension}");
        
        var isValidScan = await CheckScanFile(path);
        Console.WriteLine($"Scan Checked: {isValidScan}");
        
        var isValid = isValidExtension && isValidScan;
        Console.WriteLine($"le document est-il valide ? : {isValid}");

        if (isValid) {
            await MoveToEprolex_File(document!);
            Console.WriteLine("Document valide déplacé dans le File System définitif [EPROLEX_FILE]");
        }

        await PutStatutAndAddress(isValid, document!);
        Console.WriteLine("Mise à jour de la DB");

    }

    private async Task<Document?> GetDocument(int documentId) {

        var httpClient = factory.CreateClient("api");
        await Task.Delay(1000);

        return await httpClient.GetFromJsonAsync<Document>($"/document/{documentId}");
    }

    private async Task<string> CalculatePath(Document document) {
        
        await Task.Delay(1000);

        return "path";
    }

    private async Task<bool> CheckScanFile(string filePath) {

        
        for (var i = 0; i < 12; i++) {
            Console.Write(". ");
            await Task.Delay(400);
        }
        Console.WriteLine(". ");
        
        // SCAN FILE

        return new Random().Next(1, 9) != 5;
    }
    
    private async Task<bool> CheckExtension(string filePath) {
        
        await Task.Delay(1000);
        
        // CHECK EXTENSION

        return new Random().Next(1, 9) != 5;
    }
    
    private async Task MoveToEprolex_File(Document document) {
        // MOVE TO EPROLEX_FILE

        await Task.Delay(1000);
    }

    private async Task PutStatutAndAddress(bool isValid, Document document) {
        
        var statut = isValid ? "Valide" : "Corrompu";

        document.StatutCode = statut;
        document.Addresse = statut == "Valide" 
            ? "address définitive dans EPROLEX_FILE" 
            : "Pas d'adresse";
        
        var httpClient = factory.CreateClient("api");
        await Task.Delay(1000);

        await httpClient.PutAsJsonAsync<Document>($"/document", document);
    }

    
}