namespace FileScanWorker.Repositories;

public class DocumentRepository(IHttpClientFactory factory)
{
    public async Task<Document?> GetByIdAsync(int documentId) {

        var httpClient = factory.CreateClient("api");
        await Task.Delay(600);

        return await httpClient.GetFromJsonAsync<Document>($"/document/{documentId}");
    }
    
    public async Task UpdateStatutAndAddressAsync( Document document) {

        document.Chemin = document.StatutCode == "Valide" 
            ? "chemin définitive dans EPROLEX_FILE" 
            : "Pas de chemin";
        
        var httpClient = factory.CreateClient("api");
        await Task.Delay(600);

        // TODO: Utiliser un  dto avec seulement documentId et statut
        await httpClient.PutAsJsonAsync($"/document", document);
    }
}