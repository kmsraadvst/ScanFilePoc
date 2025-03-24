using Domain.Contracts;
using Domain.HubEvents;
using Microsoft.AspNetCore.SignalR.Client;

namespace FileScanWorker.Services;

public class SignalRClientService : IAsyncDisposable
{
    private HubConnection _connection = new HubConnectionBuilder()
        .WithUrl("http://localhost:5216/my-hub")
        .WithAutomaticReconnect()
        .Build();

    private async Task StartSignalRConnection()
    {
        if (_connection.State == HubConnectionState.Disconnected)
        {
            await _connection.StartAsync();
        }
    }
    
    public async Task SendStatutUpdated(DocumentStatutUpdatedMessage message)
    {
        await StartSignalRConnection();

        // await _connection.SendAsync(MyHubEvents.ReceiveUpdatedDocument, message);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}