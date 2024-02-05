using StackExchange.Redis;

namespace API.SignalR;

public class PresenceTracker
{

    private readonly IDatabase _db;
    private const string HASH_KEY = "online";

    public PresenceTracker(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<bool> UserConnectedAsync(string username, string connectionId)
    {
        var connectionCount = await _db.HashIncrementAsync(HASH_KEY, username, 1);
        return connectionCount == 1;
    }

    public async Task<bool> UserDisconnectedAsync(string username, string connectionId)
    {
        var connectionCount = await _db.HashIncrementAsync(HASH_KEY, username, -1);
        return connectionCount == 0;
    }

    public async Task<List<string>> GetOnlineUsersAsync(List<string> users)
    {
        var connections = await _db.HashGetAsync(HASH_KEY, users.Select(user => (RedisValue)user).ToArray());
        return connections
         .Select((connection, index) => connection.HasValue ? users[index] : null)
         .Where(c => c != null)
         .ToList();
    }


    public Task<string[]> GetConnectionsForUserAsync(string username)
    {
        // This method needs to be removed later
        throw new NotImplementedException();
    }
}
