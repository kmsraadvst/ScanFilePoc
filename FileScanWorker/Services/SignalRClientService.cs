using Domain.Enums;
using Microsoft.AspNetCore.SignalR.Client;

namespace FileScanWorker.Services;

public class SignalRClientService : IAsyncDisposable
{
    private readonly HubConnection _connection = new HubConnectionBuilder()
        .WithUrl("http://localhost:5216/my-hub")
        .WithAutomaticReconnect()
        .Build();

    public async Task StartSignalRConnection()
    {
        if (_connection.State != HubConnectionState.Disconnected)
        {
            return;
        }
        await _connection.StartAsync();
    }
    
    public async Task SendDocumentUpdated(Document document)
    {
        var notification = new DocumentStatutUpdatedNotification
        (
            DemandeAvisId: document.DemandeAvisId,
            DocumentId: document.Id,
            DocumentStatut: document.StatutCode,
            TypeDocument: document.TypeCode
        );
        
        // await StartSignalRConnection();

        Console.WriteLine($"Avant l'envoie de la notification {notification}");
        await _connection.SendAsync(HubMethods.DocumentStatutUpdated, notification);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}