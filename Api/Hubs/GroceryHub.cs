using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

[Authorize]
public class GroceryHub : Hub
{
    public const string ChangedMethod = "groceryChanged";
}
