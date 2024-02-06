using API.Data;
using API.Data.Repositories;
using API.Entities;
using API.Helpers;
using API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Tests.TestHelpers;


namespace Tests.Data;

[TestFixture]
public class UserRepositoryTests
{
    private DataContext _context;
    private IUserRepository _userRepository;
    private List<AppUser> _users;

    [SetUp]
    public void SetUp()
    {
        _users = DataHelpers.GetUsers();
        _context = DataHelpers.GetDatabase();
        _userRepository = new UserRepository(_context, DataHelpers.GetMapperProfile());
    }

    [Test]
    public void Update_ShouldTrackingUser()
    {
        // Arrange
        var user = _context.Users.AsNoTracking().First();
        user.Id = 0;

        // Act
        _userRepository.Update(user);

        // Assert
        Assert.That(_context.ChangeTracker.HasChanges(), Is.EqualTo(true));
    }

    [Test]
    public async Task GetUsersAsync_ShouldReturnExistUsers()
    {
        // Act
        var act = (await _userRepository.GetUsersAsync()).ToList();

        // Assert
        Assert.That(act.Count, Is.EqualTo(_users.Count));
        for (int i = 0; i < act.Count; i++)
        {
            Assert.That(act[i].UserName, Is.EqualTo(_users[i].UserName));
        }
    }


    [TestCase(1)]
    [TestCase(2)]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUser(int id)
    {
        // Act
        var act = await _userRepository.GetUserByIdAsync(id);

        // Assert
        Assert.That(act.UserName, Is.EqualTo(_users[id - 1].UserName));
    }

    [TestCase(111)]
    public async Task GetUserByIdAsync_WithInvalidId_ReturnsNull(int id)
    {
        // Act
        var act = await _userRepository.GetUserByIdAsync(id);

        // Assert
        Assert.That(act, Is.EqualTo(null));
    }

    [TestCase("lisa")]
    [TestCase("todd")]
    public async Task GetUserByUsernameAsync_WithValidUsername_ReturnsUser(string username)
    {
        // Act
        var act = await _userRepository.GetUserByUsernameAsync(username);

        // Assert
        Assert.That(act.UserName, Is.EqualTo(username));
    }

    [TestCase("invalidName")]
    public async Task GetUserByUsernameAsync_WithInvalidUsername_ReturnsNull(string username)
    {
        // Act
        var act = await _userRepository.GetUserByUsernameAsync(username);

        // Assert
        Assert.That(act, Is.EqualTo(null));
    }

    [TestCase("lisa")]
    [TestCase("todd")]
    public async Task GetMembersAsync_WithParamsCurrentUserName_ReturnsUsersWithoutCurrentUser(string name)
    {
        // Arrange
        var userParams = new UserParams { CurrentUserName = name };

        // Act
        var act = await _userRepository.GetMembersAsync(userParams);

        // Assert
        Assert.That(act.Select(x => x.UserName).Contains(name), Is.EqualTo(false));
    }

    [TestCase("male")]
    [TestCase("female")]
    public async Task GetMembersAsync_WithParamsGender_ReturnsUsersWithoutLisa(string gender)
    {
        // Arrange
        var userParams = new UserParams { Gender = gender };

        // Act
        var act = await _userRepository.GetMembersAsync(userParams);

        // Assert
        Assert.That(act.All(x => x.Gender == gender), Is.EqualTo(true));
    }


    [TestCase("lisa")]
    [TestCase("todd")]
    public async Task GetMemberAsync_WithValidUsername_ReturnsMember(string username)
    {
        // Act
        var act = await _userRepository.GetMemberAsync(username);

        // Assert
        Assert.That(act.UserName, Is.EqualTo(username));
    }

    [TestCase("invalidName")]
    public async Task GetMemberAsync_WithInvalidUsername_ReturnsNull(string username)
    {
        // Act
        var act = await _userRepository.GetMemberAsync(username);

        // Assert
        Assert.That(act, Is.EqualTo(null));
    }

    [TestCase("lisa", "female")]
    [TestCase("todd", "male")]
    [TestCase("invalidName", null)]
    public async Task GetUserGenderAsync_WithValidUsername_ReturnsGender(string username, string expected)
    {
        // Act
        var act = await _userRepository.GetUserGenderAsync(username);

        // Assert
        Assert.That(act, Is.EqualTo(expected));
    }
}
