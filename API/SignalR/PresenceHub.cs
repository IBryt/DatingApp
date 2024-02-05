using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class PresenceHub : Hub
{
    private readonly PresenceTracker _presenceTracker;

    public PresenceHub(PresenceTracker presenceTracker)
    {
        _presenceTracker = presenceTracker;
    }

    public override async Task OnConnectedAsync()
    {
        var isOnline = await _presenceTracker.UserConnectedAsync(Context.User.GetUsername(), Context.ConnectionId);
        if (isOnline)
        {
            await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var isOffline = await _presenceTracker.UserDisconnectedAsync(Context.User.GetUsername(), Context.ConnectionId);
        if (isOffline)
        {
            await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task GetOnlineUsers(List<string> users)
    {
        var currentUsers = await _presenceTracker.GetOnlineUsersAsync(users);

        await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
    }
}
