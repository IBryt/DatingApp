using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Moq;
using NUnit.Framework;
using System.Text;
using Tests.TestHelpers;


namespace Tests.IntegrationControllerTests;

public class UsersIntegrationTest
{
    private CustomWebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private const string RequestUri = "api/users";
    private string _lisaToken;

    [OneTimeSetUp]
    public async Task Init()
    {
        using var factory = new CustomWebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        _lisaToken = await IntegrationTestsHelpers.GetToken("lisa", "Pa$$w0rd", factory, client);
    }


    [SetUp]
    public void Setup()
    {
        var imageUploadDtoMock = new Mock<ImageUploadDto>();
        var photoDeletionResultDtoMock = new Mock<PhotoDeletionResultDto>();
        var photoServiceMock = new Mock<IPhotoService>();
        photoServiceMock.
            Setup(x => x.AddPhotoAsync(It.IsAny<IFormFile>())).ReturnsAsync(imageUploadDtoMock.Object);

        photoServiceMock.
            Setup(x => x.DeletePhotoAsync(It.IsAny<string>())).ReturnsAsync(photoDeletionResultDtoMock.Object);

        _factory = new CustomWebApplicationFactory<Program>() { PhotoService = photoServiceMock.Object };
        _client = _factory.CreateClient();

    }

    [TearDown]
    public void TearDown()
    {
        _factory.Dispose();
        _client.Dispose();
    }

    [Test]
    public async Task GetUser_WithParamsCurrentUserNameShouldReturnsListUsersWithoutCurrentUser()
    {
        for (int i = 1; i <= 2; i++)
        {
            // Arrange
            var currentUserName = "lisa";
            var request = new HttpRequestMessage(HttpMethod.Get, RequestUri + $"?currentUserName={currentUserName}&pageNumber={i}");
            request.Headers.Add("Authorization", "Bearer " + _lisaToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            var actual = await IntegrationTestsHelpers.DeserializeResponse<IEnumerable<MemberDto>>(response);
            Assert.That(actual.FirstOrDefault(x => x.UserName == currentUserName), Is.Null);
        }
    }

    [Test]
    public async Task GetUser_WithParamsGenderShouldReturnsListUsersWithSpecificGender()
    {
        var genders = new string[] { "male", "female" };
        foreach (var gender in genders)
        {
            for (int i = 1; i <= 2; i++)
            {
                // Arrange
                var request = new HttpRequestMessage(HttpMethod.Get, RequestUri + $"?gender={gender}&pageNumber={i}");
                request.Headers.Add("Authorization", "Bearer " + _lisaToken);

                // Act
                var response = await _client.SendAsync(request);

                // Assert
                var actual = await IntegrationTestsHelpers.DeserializeResponse<IEnumerable<MemberDto>>(response);
                Assert.That(actual.All(x => x.Gender == gender), Is.True);
            }
        }
    }

    [Test]
    public async Task GetUser_WithParamsMinAgeShouldReturnsListUsersWithLessAge()
    {
        for (int i = 1; i <= 2; i++)
        {
            // Arrange
            var minAge = 56;
            var request = new HttpRequestMessage(HttpMethod.Get, RequestUri + $"?minAge={minAge}&pageNumber={i}");
            request.Headers.Add("Authorization", "Bearer " + _lisaToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            var actual = await IntegrationTestsHelpers.DeserializeResponse<IEnumerable<MemberDto>>(response);
            Assert.That(actual.Where(x => x.Age >= minAge).Count, Is.EqualTo(actual.Count()));
        }
    }

    [Test]
    public async Task GetUser_WithParamsMinAgeShouldReturnsListUsersWithMaxAge()
    {
        for (int i = 1; i <= 2; i++)
        {
            // Arrange
            var maxAge = 56;
            var request = new HttpRequestMessage(HttpMethod.Get, RequestUri + $"?maxAge={maxAge}&pageNumber={i}");
            request.Headers.Add("Authorization", "Bearer " + _lisaToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            var actual = await IntegrationTestsHelpers.DeserializeResponse<IEnumerable<MemberDto>>(response);
            Assert.That(actual.Where(x => x.Age <= maxAge).Count, Is.EqualTo(actual.Count()));
        }
    }

    [Test]
    public async Task GetUser_WithParamsOrderByShouldReturnsListUsersSortedByLastActive()
    {
        for (int i = 1; i <= 2; i++)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, RequestUri + $"?orderBy=lastActive&pageNumber={i}");
            request.Headers.Add("Authorization", "Bearer " + _lisaToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            var actual = await IntegrationTestsHelpers.DeserializeResponse<IEnumerable<MemberDto>>(response);
            Assert.That(actual.OrderByDescending(x => x.LastActive), Is.EqualTo(actual));
        }
    }

    [Test]
    public async Task GetUser_WithParamsOrderByShouldReturnsListUsersSortedByCreated()
    {
        for (int i = 1; i <= 2; i++)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, RequestUri + $"?orderBy=created&pageNumber={i}");
            request.Headers.Add("Authorization", "Bearer " + _lisaToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            var actual = await IntegrationTestsHelpers.DeserializeResponse<IEnumerable<MemberDto>>(response);
            Assert.That(actual.OrderByDescending(x => x.Created), Is.EqualTo(actual));
        }
    }

    [Test]
    public async Task GetMemberAsync_ShouldReturnsMemberDtoWithSameName()
    {
        // Arrange
        var username = "lisa";
        var request = new HttpRequestMessage(HttpMethod.Get, RequestUri + $"/{username}");
        request.Headers.Add("Authorization", "Bearer " + _lisaToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        var actual = await IntegrationTestsHelpers.DeserializeResponse<MemberDto>(response);
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.UserName, Is.EqualTo(username));
    }

    [Test]
    public async Task UpdateUser_ShouldReturnsMemberDtoWithUpdatedProperies()
    {
        // Arrange
        var username = "lisa";
        var request = new HttpRequestMessage(HttpMethod.Put, RequestUri);
        request.Headers.Add("Authorization", "Bearer " + _lisaToken);
        var memberUpdateDto = new MemberUpdateDto
        {
            Introduction = "Introduction",
            LookingFor = "LookingFor",
            Interests = "Interests",
            City = "City",
            Country = "Country",
        };
        request.Content = IntegrationTestsHelpers.StringContent(memberUpdateDto);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        using var scope = _factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var member = context.Users.FirstOrDefault(x => x.UserName == username);

        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(member.UserName, Is.EqualTo(username));
        Assert.That(member.Introduction, Is.EqualTo(memberUpdateDto.Introduction));
        Assert.That(member.LookingFor, Is.EqualTo(memberUpdateDto.LookingFor));
        Assert.That(member.Interests, Is.EqualTo(memberUpdateDto.Interests));
        Assert.That(member.City, Is.EqualTo(memberUpdateDto.City));
        Assert.That(member.Country, Is.EqualTo(memberUpdateDto.Country));
    }



    [Test]
    public async Task AddPhoto_ReturnsCreatedResponse()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, RequestUri + "/add-photo");
        request.Headers.Add("Authorization", "Bearer " + _lisaToken);
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("file content"))));
        request.Content = content;

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    [Test]
    public async Task DeletePhoto_ShouldDeleteFromDatabase()
    {
        // Arrange
        var username = "lisa";
        using var scope = _factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var lisa = context.Users
            .Include(u => u.Photos)
            .First(x => x.UserName == username);


        var photo = new Photo { PublicId = "PublicId" };
        lisa.Photos.Add(photo);
        context.SaveChanges();
        context.Entry(lisa).State = EntityState.Detached;
        context.Entry(photo).State = EntityState.Detached;

        var request = new HttpRequestMessage(HttpMethod.Delete, RequestUri + $"/delete-photo/{photo.Id}");
        request.Headers.Add("Authorization", "Bearer " + _lisaToken);
        // Act
        var response = await _client.SendAsync(request);

        // Assert
        lisa = context.Users
            .Include(u => u.Photos)
            .First(x => x.UserName == username);

        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(lisa.Photos.FirstOrDefault(p => p.Id == photo.Id)?.Id, Is.Null);
    }

    [Test]
    public async Task DeletePhoto_ShouldReturnsBadRequestIfIsMainPhoto()
    {
        // Arrange
        var username = "lisa";
        using var scope = _factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var lisa = context.Users
            .Include(u => u.Photos)
            .First(x => x.UserName == username);
        var photo = lisa.Photos.Single(x => x.IsMain);


        var request = new HttpRequestMessage(HttpMethod.Delete, RequestUri + $"/delete-photo/{photo.Id}");
        request.Headers.Add("Authorization", "Bearer " + _lisaToken);
        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.False);
    }
}
