using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository : IUserRepository
{
    private readonly DataContext _contex;

    public UserRepository(DataContext contex)
    {
        _contex = contex;
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {
        return await _contex.Users.FindAsync(id);
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
        return await _contex.Users
            .Include(u => u.Photos)
            .SingleOrDefaultAsync(x => x.UserName == username) ;
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await _contex.Users
            .Include(u => u.Photos)
            .ToListAsync();
    }

    public async Task<bool> SaveAllAsync()
    {
        return await _contex.SaveChangesAsync() > 0;
    }

    public void Update(AppUser user)
    {
        _contex.Entry(user).State = EntityState.Modified;
    }
}
