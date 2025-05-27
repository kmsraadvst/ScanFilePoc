using Domain.Dtos;

namespace FileScanWorker.Services;

public class FileScanService(DocumentRepository repo, IHttpClientFactory factory)
{
    public async Task<bool> ScanAsync(Document document)
    {
        Console.WriteLine($"Scanning file {document.Chemin}");
        var retryCount = 0;
        var httpClient = factory.CreateClient("scan");

        httpClient.Timeout = TimeSpan.FromSeconds(2);
        
        // SCAN FILE
        while (true)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("/scan-file", new FilePathDto(document.Chemin));
                var result = await response.Content.ReadFromJsonAsync<ResultScanDto>() ??
                             throw new ScanFileApiException($"Erreur avec L'API de scan de fichier result est null");

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

    public async Task<bool> ScanFakeAsync()
    {
        await Task.Delay(600);

        return new Random().Next(1, 10) <= 8;
    }
    
}