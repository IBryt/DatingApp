namespace API.Interfaces.Repositories;

public interface IGroupRepository
{
    Task AddAsync(string connectionId, string groupName);
    Task<string> GetAsync(string connectionId);
    Task DeleteAsync(string connectionId);
}
