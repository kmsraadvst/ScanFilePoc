using Domain.Enums;
using Microsoft.AspNetCore.SignalR.Client;

namespace FileScanWorker.Services;

public class SignalRClientService<TNotification> : IAsyncDisposable where TNotification : class
{
    private readonly HubConnection _connection = new HubConnectionBuilder()
        .WithUrl("http://localhost:5216/my-hub")
        .WithAutomaticReconnect()
        .Build();

    private readonly SemaphoreSlim _semaphore = new(1);

    public SignalRClientService()
    {
        _connection.Reconnected += connectionId =>
        {
            Console.WriteLine($"signalr reconnected connectionId:{connectionId}");
        
            return Task.CompletedTask;
        };
        
        _connection.Reconnecting += e =>
        {
            Console.WriteLine($"signalr is reconnecting exception message:{e?.Message}");
        
            return Task.CompletedTask;
        };
        
        _connection.Closed += e =>
        {
            Console.WriteLine($"signalr is closed exception message:{e?.Message}");

            return Task.CompletedTask;
        };
    }

    private async Task EnsureSignalRConnectionStarted()
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
                Console.WriteLine("SignalR is started");
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task NotifyAsync(string hubMethod, TNotification notification)
    {
        await EnsureSignalRConnectionStarted();

        Console.WriteLine($"Avant l'envoie de la notification {notification}");
        await _connection.SendAsync(hubMethod, notification);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        _semaphore.Dispose();
    }
}