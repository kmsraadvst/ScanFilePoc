using Domain.Entities;
using Domain.Enums;
using Domain.HubEvents;

namespace BlazorServerUI.Repositories;

public class DocumentRepository(IHttpClientFactory factory, IHubContext<MyHub> context)
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

        var response = await client.PostAsJsonAsync("/Document", documentToCreate);

        Console.WriteLine($"response status code: {response.StatusCode}");

        await context.Clients.Group($"{documentToCreate.DemandeAvisId}")
            .SendAsync(GetEventHub(documentToCreate.TypeCode));
        Console.WriteLine($"Envoie de l'event hub : {GetEventHub(documentToCreate.TypeCode)}");

        return await response.Content.ReadFromJsonAsync<Document>();
    }

    public async Task<int> DeleteAsync(int id) {
        var client = factory.CreateClient("api");

        var documentToDelete = await GetByIdAsync(id);

        if (documentToDelete is null) return 0;
        
        var rowsAffected = await client.DeleteFromJsonAsync<int>($"/Document/{id}");
        
        await context.Clients.Group($"{documentToDelete.DemandeAvisId}")
            .SendAsync(GetEventHub(documentToDelete.TypeCode));
        
        return rowsAffected;
    }
    
    private string GetEventHub(string type) => type switch
    {
        _ when type == TypeDocument.Projet => MyHubEvents.RefreshProjet,
        _ when type == TypeDocument.Mandat => MyHubEvents.RefreshMandat,
        _ when type == TypeDocument.AnnexeProjet => MyHubEvents.RefreshAnnexe,
        _ => throw new ArgumentOutOfRangeException()
    };
}