using System.Text.Json;
using Domain.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace BlazorServerUI.Hubs;

public class MyHub : Hub
{
    public async Task ReceivedDocumentStatutUpdated(DocumentStatutUpdatedMessage message)
    {
        var messageStr = JsonSerializer.Serialize(message, JsonSerializerOptions.Web);
    }
}