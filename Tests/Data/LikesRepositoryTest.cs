using API.Data;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using NUnit.Framework;
using Tests.TestHelpers;

namespace Tests.Data;

public class LikesRepositoryTest
{
    private DataContext _context;
    private ILikesRepository _likesRepository;
    private List<AppUser> _users;
    private AppUser _sourceUser;
    private AppUser _likedUser;

    [SetUp]
    public void SetUp()
    {
        _users = DataHelpers.GetUsers();
        _context = DataHelpers.GetDatabase();
        _likesRepository = new LikesRepository(_context);
        _sourceUser = _context.Users.Find(1);
        _likedUser = _context.Users.Find(2);
    }


    [Test]
    public async Task AddLikeAsync_ShouldContainsEntry()
    {
        // Arrange
        var userLike = NewUserLike();

        // Act
        await _likesRepository.AddLikeAsync(userLike);
        _context.SaveChanges();

        // Assert
        Assert.That(_context.Likes.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetUserLikeAsync_ShouldReturnUserLike()
    {
        // Arrange
        var userLike = NewUserLike();
        await _likesRepository.AddLikeAsync(userLike);
        _context.SaveChanges();

        // Act
        await _likesRepository.GetUserLikeAsync(_sourceUser.Id, _likedUser.Id);

        // Assert
        Assert.That(_context.Likes.First().SourceUser, Is.EqualTo(userLike.SourceUser));
        Assert.That(_context.Likes.First().LikedUser, Is.EqualTo(userLike.LikedUser));
    }

    [Test]
    public async Task GetUserWithLikesAsync_ShouldReturnUserLike()
    {
        // Arrange
        var userLike = NewUserLike();
        await _likesRepository.AddLikeAsync(userLike);
        _context.SaveChanges();

        // Act
        var act = await _likesRepository.GetUserWithLikesAsync(_sourceUser.Id);

        // Assert
        Assert.That(act.LikedUsers.First().LikedUser, Is.EqualTo(_likedUser));
    }

    [TestCase("liked", 1, 1)]
    [TestCase("likedBy", 0, 1)]
    public async Task GetUserLikesAsync_ShouldReturnUserLike(string predicate, int expectedCount, int id)
    {
        // Arrange
        var userLike = new UserLike
        {
            SourceUser = _sourceUser,
            SourceUserId = _sourceUser.Id,
            LikedUser = _likedUser,
            LikedUserId = _likedUser.Id
        };
        await _likesRepository.AddLikeAsync(userLike);
        _context.SaveChanges();

        var likesParams = new LikesParams { UserId = id, Predicate = predicate };

        // Act
        var act = await _likesRepository.GetUserLikesAsync(likesParams);

        // Assert
        Assert.That(act.Count, Is.EqualTo(expectedCount));
    }

    private UserLike NewUserLike()
    {
        return new UserLike
        {
            SourceUser = _sourceUser,
            SourceUserId = _sourceUser.Id,
            LikedUser = _likedUser,
            LikedUserId = _likedUser.Id
        };
    }
}
