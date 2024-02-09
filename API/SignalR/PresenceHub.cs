using API.Extensions;
using API.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class PresenceHub : Hub
{
    private readonly IPresenceRepository _presenceRepository;

    public PresenceHub(IPresenceRepository presenceRepository)
    {
        _presenceRepository = presenceRepository;
    }

    public override async Task OnConnectedAsync()
    {
        var isOnline = await _presenceRepository.UserConnectedAsync(Context.User.GetUsername(), Context.ConnectionId);
        if (isOnline)
        {
            await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var isOffline = await _presenceRepository.UserDisconnectedAsync(Context.User.GetUsername(), Context.ConnectionId);
        if (isOffline)
        {
            await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task GetOnlineUsers(List<string> users)
    {
        var currentUsers = await _presenceRepository.GetOnlineUsersAsync(users);

        await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
    }
}
