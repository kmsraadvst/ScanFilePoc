using Domain.Entities;

namespace BlazorServerUI.Repositories;

public class DocumentRepository(IHttpClientFactory factory)
{
    public async Task<Document?> GetByIdAsync(int id) {
        var client = factory.CreateClient("api");

        return await client.GetFromJsonAsync<Document>($"/Document/{id}");
    }

    public async Task<IEnumerable<Document>> GetAllAsync(string typeDocument, int demandeAvisId) {
        var client = factory.CreateClient("api");

        // demande-avis/124/document?typeCode=Projet
        return await client.GetFromJsonAsync<IEnumerable<Document>>(
            $"/demande-avis/{demandeAvisId}/document?typeCode={typeDocument}"
        ) ?? [];
    }

    public async Task<Document?> CreateAsync(Document documentToCreate) {
        var client = factory.CreateClient("api");

        var response = await client.PostAsJsonAsync<Document>("/Document", documentToCreate);

        Console.WriteLine($"response status code: {response.StatusCode}");


        return await response.Content.ReadFromJsonAsync<Document>();
    }

    public async Task<int> DeleteAsync(int id) {
        var client = factory.CreateClient("api");

        return await client.DeleteFromJsonAsync<int>($"/Document/{id}");
    }
}