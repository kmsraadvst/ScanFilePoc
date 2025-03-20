using Domain.Entities;

namespace BlazorServerUI.Repositories;

public class DocumentRepository(IHttpClientFactory factory)
{
    public async Task<Document?> GetByIdAsync(int id)
    {
        var client = factory.CreateClient("api");

        return await client.GetFromJsonAsync<Document>($"/Document/{id}");
    }
    
    public async Task<IEnumerable<Document>> GetAllAsync()
    {
        var client = factory.CreateClient("api");

        return await client.GetFromJsonAsync<IEnumerable<Document>>($"/Document") ?? [];
    }
    
    public async Task<Document?> CreateAsync(Document documentToCreate)
    {
        var client = factory.CreateClient("api");

        var response = await client.PostAsJsonAsync<Document>("/Document", documentToCreate);

        return await response.Content.ReadFromJsonAsync<Document>();
    }
    
    public async Task<int> DeleteAsync(int id)
    {
        var client = factory.CreateClient("api");

        return await client.DeleteFromJsonAsync<int>($"/Document/{id}");
    }
}