using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces.Repositories;

public interface ILikesRepository
{
    Task<UserLike> GetUserLikeAsync(int sourceUserId, int likedUserId);
    Task<AppUser> GetUserWithLikesAsync(int userId);
    Task<PagedList<LikeDto>> GetUserLikesAsync(LikesParams likesParams);
    Task AddLikeAsync(UserLike entity);
}
