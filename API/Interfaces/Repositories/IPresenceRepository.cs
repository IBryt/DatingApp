namespace API.Interfaces.Repositories;

public interface IPresenceRepository
{
    Task<bool> UserConnectedAsync(string username, string connectionId);
    Task<bool> UserDisconnectedAsync(string username, string connectionId);
    Task<List<string>> GetOnlineUsersAsync(List<string> users);
    Task<bool> IsOnlineAsync(string username);
}