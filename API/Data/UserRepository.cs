using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UserRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MemberDto> GetMemberAsync(string username)
    {
        return await _context.Users
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {

        var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1).ToUniversalTime();
        var maxDob = DateTime.Today.AddYears(-userParams.MinAge).ToUniversalTime();

        var query = _context.Users.AsQueryable();

        query = query.Where(
            u => u.UserName != userParams.CurrentUserName
            && u.Gender == userParams.Gender
            && u.DateOfBirth >= minDob
            && u.DateOfBirth <= maxDob);

        query = userParams.OrderBy switch
        {
            "created" => query.OrderByDescending(u => u.Created),
            _ => query.OrderByDescending(u => u.LastActive)
        };

        var source = query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .AsNoTracking();

        return await PagedList<MemberDto>.CreateAsync(source, userParams.PageNumber, userParams.PageSize);
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Photos)
            .SingleOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<string> GetUserGenderAsync(string username)
    {
        return await _context.Users
            .Where(u => u.UserName == username)
            .Select(u => u.Gender)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Photos)
            .ToListAsync();
    }

    public void Update(AppUser user)
    {
        _context.Entry(user).State = EntityState.Modified;
    }
}
