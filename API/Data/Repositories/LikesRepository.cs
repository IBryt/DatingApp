using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories;

public class LikesRepository : ILikesRepository
{
    private readonly DataContext _context;

    public LikesRepository(DataContext context)
    {
        _context = context;
    }

    public async Task AddLikeAsync(UserLike userLike)
    {
        await _context.Likes.AddAsync(userLike);
    }

    public async Task<UserLike> GetUserLikeAsync(int sourceUserId, int likedUserId)
    {
        return await _context.Likes.FindAsync(sourceUserId, likedUserId);
    }

    public async Task<PagedList<LikeDto>> GetUserLikesAsync(LikesParams likesParams)
    {
        if (string.IsNullOrEmpty(likesParams.Predicate))
        {
            throw new ArgumentException("This cannot be empty", likesParams.Predicate);
        }

        var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
        var likes = _context.Likes.AsQueryable();

        if (likesParams.Predicate == "liked")
        {
            likes = likes.Where(l => l.SourceUserId == likesParams.UserId);
            users = likes.Select(l => l.LikedUser);
        }

        if (likesParams.Predicate == "likedBy")
        {
            likes = likes.Where(l => l.LikedUserId == likesParams.UserId);
            users = likes.Select(l => l.SourceUser);
        }

        var likedUsers = users.Select(user => new LikeDto
        {
            UserName = user.UserName,
            Age = user.DateOfBirth.CalculateAge(),
            KnownAs = user.KnownAs,
            PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
            City = user.City,
            Id = user.Id,
        });
        return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
    }

    public async Task<AppUser> GetUserWithLikesAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.LikedUsers)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
}
