using Domain.Dtos;
using Domain.Exceptions;

namespace FileScanWorker.Services;

public class FileScanService(DocumentRepository repo, IHttpClientFactory factory)
{
    public async Task<Document> DocumentScanProcess(DocumentToScanMessage message)
    {
        Console.WriteLine("Démarer le processus SCAN FILE");

        Document? document;

        // 1. RÉCUPÉRER LE DOCUMENT EN DB
        try
        {
            document = await repo.GetByIdAsync(message.DocumentId);

            if (document is null)
            {
                Console.WriteLine($"Le document Id[{message.DocumentId}] n'existe pas");
                throw new GetDocumentException($"Le document Id[{message.DocumentId}] n'existe pas");
            }

            Console.WriteLine($"FileScan reçoit de l'API Document Id[{document.Id}]");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Erreur GET pour chercher le document: {e.Message}");
            throw new GetDocumentException($"Erreur GET pour chercher le document: {e.Message}", e.InnerException);
        }

        // 2. CALCULER LE CHEMIN DU FICHIER
        var path = await CalculatePath(document);
        Console.WriteLine("chemin calculé");

        // 3. VÉRIFIER L'EXTENSION
        var isValidExtension = await CheckExtension(path);
        Console.WriteLine($"Extension vérifiée isValidExtension={isValidExtension}");

        // 4. SCANNER LE FICHIER POUR DÉTECTER DES MALWARES
        var isValidScan = await CheckScanFile(path);
        Console.WriteLine($"Scan (Antivirus) vérifié isValidScan={isValidScan}");

        var isValid = isValidExtension && isValidScan;
        Console.WriteLine($"le document est-il valide ? : {isValid}");

        // 5. SI LE DOCUMENT EST VALIDE LE DÉPLACER VERS LE FILE SYSTEM DE PROLEX
        if (isValid)
        {
            await MoveToEprolex_File(document);
            Console.WriteLine("Document valide déplacé dans le File System définitif [EPROLEX_FILE]");
        }

        // 6. METTRE À JOUR LE DOCUMENT EN DB STATUT ET CHEMIN
        document.StatutCode = isValid ? "Valide" : "Corrompu";

        try
        {
            await repo.UpdateStatutAndAddressAsync(document);
            Console.WriteLine("Mise à jour de la DB");
            Console.WriteLine("Fin du processus SCAN FILE");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Erreur PUT de mise à jour de la DB: {e.Message}");
            throw new PutDocumentException($"Erreur PUT de mise à jour de la DB: {e.Message}", e.InnerException);
        }

        return document;
    }


    private async Task<string> CalculatePath(Document document)
    {
        await Task.Delay(300);
        Console.WriteLine($"path is calculated for {document.NomFichier}");
        return "path";
    }

    private async Task<bool> CheckScanFile(string filePath)
    {
        Console.WriteLine($"Scanning file {filePath}");
        var retryCount = 0;
        var httpClient = factory.CreateClient("scan");

        httpClient.Timeout = TimeSpan.FromSeconds(2);

        for (var i = 0; i < 6; i++)
        {
            Console.Write(". ");
            await Task.Delay(100);
        }

        Console.WriteLine(". ");

        // SCAN FILE

        while (true)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("/scan-file", new FilePathDto(filePath));
                var result = await response.Content.ReadFromJsonAsync<ResultScanDto>() ??
                             throw new ScanFileApiException($"Erreur avec L'API de scan de fichier result est null");
                ;

                return result.FileIsValid;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erreur avec L'API de scan de fichier {e.Message}");
                if (retryCount <= 3)
                {
                    // var delay = Convert.ToInt32(Math.Pow(10, retryCount));
                    var delay = 2 * retryCount;
                    Console.WriteLine($"attendre {delay} s avant de réessayer");
                    await Task.Delay(TimeSpan.FromSeconds(delay));
                    retryCount++;
                }
                else
                {
                    throw new ScanFileApiException($"Erreur avec L'API de scan de fichier {e.Message}",
                        e.InnerException);
                }
            }
        }
    }

    private async Task<bool> CheckExtension(string filePath)
    {
        await Task.Delay(600);

        // CHECK EXTENSION

        return new Random().Next(1, 9) != 5;
    }

    private async Task MoveToEprolex_File(Document document)
    {
        // MOVE TO EPROLEX_FILE

        await Task.Delay(600);
    }
}