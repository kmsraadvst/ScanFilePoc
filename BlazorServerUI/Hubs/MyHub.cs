
namespace BlazorServerUI.Hubs;

public class MyHub : Hub
{
    public async Task AddToGroup(string groupName)
    {
        Console.WriteLine($"Adding To Group {groupName}");
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }
}