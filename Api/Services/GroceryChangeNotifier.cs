using Api.Dtos;
using Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Api.Services;

public interface IGroceryChangeNotifier
{
    Task NotifyCreatedAsync(int entryId);
    Task NotifyUpdatedAsync(int entryId);
    Task NotifyDeletedAsync(int entryId);
    Task NotifyClearedAsync();
}

public class GroceryChangeNotifier(
    IHubContext<GroceryHub> hubContext,
    ILogger<GroceryChangeNotifier> logger) : IGroceryChangeNotifier
{
    public Task NotifyCreatedAsync(int entryId) => BroadcastAsync("created", entryId);

    public Task NotifyUpdatedAsync(int entryId) => BroadcastAsync("updated", entryId);

    public Task NotifyDeletedAsync(int entryId) => BroadcastAsync("deleted", entryId);

    public Task NotifyClearedAsync() => BroadcastAsync("cleared", null);

    private async Task BroadcastAsync(string type, int? entryId)
    {
        var payload = new GroceryChangedEventDto(type, entryId, DateTime.UtcNow);

        try
        {
            await hubContext.Clients.All.SendAsync(GroceryHub.ChangedMethod, payload);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast grocery change event {Type} for entry {EntryId}.", type, entryId);
        }
    }
}
