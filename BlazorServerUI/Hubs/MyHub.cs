
namespace BlazorServerUI.Hubs;

public class MyHub : Hub
{
    public async Task DocumentStatutUpdated(DocumentStatutUpdatedNotification notification)
    {
        var eventHub = TypeDocument.GetEventHub(notification.TypeDocument);

        Console.WriteLine($"notification: {notification}");

        await Clients.Group($"{notification.DemandeAvisId}").SendAsync(eventHub);
    }
    
    public async Task AddToGroup(string groupName)
    {
        Console.WriteLine($"Adding To Group {groupName}");
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }
}