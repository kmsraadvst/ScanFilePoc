using System.Text.Json;
using Domain.HubEvents;
using Microsoft.AspNetCore.SignalR;

namespace BlazorServerUI.Hubs;

public class MyHub : Hub
{
    public async Task ReceiveUpdatedDocument(DocumentStatutUpdatedMessage message)
    {
        var messageStr = JsonSerializer.Serialize(message, JsonSerializerOptions.Web);

        Console.WriteLine(messageStr);

        // await Clients.All.SendAsync(MyHubEvents.UpdateStatut, message);
    }
}