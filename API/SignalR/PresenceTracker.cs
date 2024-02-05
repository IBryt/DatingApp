using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Collections.Generic;

namespace API.SignalR;

public class PresenceTracker
{

    private readonly IDatabase _db;
    private readonly object _lock = new object();
    private const string HASH_KEY = "hashKey";

    public PresenceTracker(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public Task<bool> UserConnectedAsync(string username, string connectionId)
    {
        var isOnline = false;

        lock (_lock)
        {
            if (_db.HashExists(HASH_KEY, username))
            {
                var currentArray = _db.HashGet(HASH_KEY, username);
                var newArray = $"{currentArray},{connectionId}";
                _db.HashSet(HASH_KEY, username, newArray);
            }
            else
            {
                _db.HashSet(HASH_KEY, username, connectionId);
                isOnline = true;
            }
        }

        return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconnectedAsync(string username, string connectionId)
    {

        var isOffline = false;

        lock (_lock)
        {
            var currentArray = _db.HashGet(HASH_KEY, username);
            var arrayValues = currentArray.ToString().Split(',');

            arrayValues = Array.FindAll(arrayValues, v => v != connectionId);

            var newArray = string.Join(",", arrayValues);

            if (string.IsNullOrEmpty(newArray))
            {
                _db.HashDelete(HASH_KEY, username);
                isOffline = true;
            }
            else
            {
                _db.HashSetAsync(HASH_KEY, username, newArray);
            }
        }

        return Task.FromResult(isOffline);
    }



    public Task<List<string>> GetOnlineUsersAsync()
    {
        List<string> onlineUsers;
        lock (_lock)
        {
            onlineUsers = _db.HashKeys(HASH_KEY).Select(x => (string)x).ToList();
        }
        return Task.FromResult(onlineUsers);
    }

    public Task<List<string>> GetOnlineUsersAsync(List<string> users)
    {
        var onlineUsers = new List<string>();
        lock (_lock)
        {
            var connectionsIds = _db.HashGet(HASH_KEY, users.Select(user => (RedisValue)user).ToArray());

            for (int i = 0; i< users.Count; i++)
            {
                if (!string.IsNullOrEmpty(connectionsIds[i]))
                {
                    onlineUsers.Add(users[i]);
                }
            }
        }
        return Task.FromResult(onlineUsers);
    }


    public Task<string[]> GetConnectionsForUserAsync(string username)
    {

        string[] connectionIds;
        lock (_lock)
        {
            connectionIds = _db.HashGet(HASH_KEY, username).ToString().Split(',').ToArray();
        }
        return Task.FromResult(connectionIds);
    }
}
