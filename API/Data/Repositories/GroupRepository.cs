using API.Interfaces.Repositories;
using StackExchange.Redis;

namespace API.Data.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly IDatabase _db;
    private const string HASH_KEY = "group";

    public GroupRepository(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task AddAsync(string connectionId, string groupName)
    {
        await _db.HashSetAsync(HASH_KEY, connectionId, groupName, flags: CommandFlags.FireAndForget);
    }

    public async Task<string> GetAsync(string connectionId)
    {
        return (await _db.HashGetAsync(HASH_KEY, connectionId)).ToString();
    }

    public async Task DeleteAsync(string connectionId)
    {
        await _db.HashDeleteAsync(HASH_KEY, connectionId, flags: CommandFlags.FireAndForget);
    }
}
