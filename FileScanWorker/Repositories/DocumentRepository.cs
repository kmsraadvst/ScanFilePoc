namespace FileScanWorker.Repositories;

public class DocumentRepository(IHttpClientFactory factory)
{
    public async Task<Document> GetByIdAsync(int documentId)
    {
        var httpClient = factory.CreateClient("api");
        await Task.Delay(600);

        return await httpClient.GetFromJsonAsync<Document>($"/document/{documentId}")
               ?? throw new GetDocumentException($"Le document Id[{documentId}] n'existe pas");
    }

    public async Task UpdatAsync(Document document)
    {
        var httpClient = factory.CreateClient("api");
        await Task.Delay(600);

        await httpClient.PutAsJsonAsync($"/document", document);
    }
}