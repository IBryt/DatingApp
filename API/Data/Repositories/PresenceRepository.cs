using API.Interfaces.Repositories;
using StackExchange.Redis;

namespace API.Data.Repositories;

public class PresenceRepository : IPresenceRepository
{

    private readonly IDatabase _db;
    private const string HASH_KEY = "online";

    public PresenceRepository(IConnectionMultiplexer redis)
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

    public async Task<bool> IsOnlineAsync(string username)
    {
        var connectionCount = (int)await _db.HashGetAsync(HASH_KEY, username);
        return connectionCount > 0;
    }
}
