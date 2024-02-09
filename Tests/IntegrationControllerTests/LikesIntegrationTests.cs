using API.Data;
using API.DTOs;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tests.TestHelpers;

namespace Tests.IntegrationControllerTests;

public class LikesIntegrationTests
{
    private CustomWebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private const string RequestUri = "api/likes";
    private string _memberToken;

    [OneTimeSetUp]
    public async Task Init()
    {
        using var factory = new CustomWebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        _memberToken = await IntegrationTestsHelpers.GetToken("todd", "Pa$$w0rd", factory, client);

    }
    [SetUp]
    public void Setup()
    {
        _factory = new CustomWebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _factory.Dispose();
        _client.Dispose();
    }


    [Test]
    public async Task AddLike_ReturnsSuccessStatusAndAddEntryInDatabase()
    {
        // Arrange
        var request = AddLike();
        using var scope = _factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));
        Assert.That(context.Likes.First(), Is.Not.Null);
    }

    [Test]
    public async Task AddLike_ReturnsSuccessBadRequest()
    {
        // Arrange
        var request1 = AddLikeIfUserLikeYourself();
        var request2 = AddLike();
        await _client.SendAsync(AddLike());

        // Act
        var response1 = await _client.SendAsync(request1);


        var response2 = await _client.SendAsync(request2);

        // Assert
        Assert.That(response1.IsSuccessStatusCode, Is.EqualTo(false));
        Assert.That(response2.IsSuccessStatusCode, Is.EqualTo(false));
    }

    private HttpRequestMessage AddLikeIfUserLikeYourself()
    {
        var username = "todd";
        var request = new HttpRequestMessage(HttpMethod.Post, RequestUri + "/" + username);
        request.Headers.Add("Authorization", "Bearer " + _memberToken);

        return request;
    }

    private HttpRequestMessage AddLike()
    {
        var username = "lisa";
        var request = new HttpRequestMessage(HttpMethod.Post, RequestUri + "/" + username);
        request.Headers.Add("Authorization", "Bearer " + _memberToken);

        return request;
    }

    [Test]
    public async Task GetUserLikes_ReturnsLikedUser()
    {
        // Arrange
        var expectedUsername = "lisa";
        await _client.SendAsync(AddLike());

        var request = new HttpRequestMessage(HttpMethod.Get, RequestUri + "?predicate=liked");
        request.Headers.Add("Authorization", "Bearer " + _memberToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));
        var actual = await IntegrationTestsHelpers.DeserializeResponse<IEnumerable<LikeDto>>(response);
        Assert.That(actual.Count, Is.EqualTo(1));
        Assert.That(actual.First().UserName, Is.EqualTo(expectedUsername));
    }

    [Test]
    public async Task GetUserLikes_ReturnsLikedByUser()
    {
        // Arrange
        var lisaToken = await IntegrationTestsHelpers.GetToken("lisa", "Pa$$w0rd", _factory, _client);
        var requestLisaLike = new HttpRequestMessage(HttpMethod.Post, RequestUri + "/todd");
        requestLisaLike.Headers.Add("Authorization", "Bearer " + lisaToken);
        await _client.SendAsync(requestLisaLike);

        var request = new HttpRequestMessage(HttpMethod.Get, RequestUri + "?predicate=likedBy");
        request.Headers.Add("Authorization", "Bearer " + _memberToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));
        var actual = await IntegrationTestsHelpers.DeserializeResponse<IEnumerable<LikeDto>>(response);
        Assert.That(actual.Count, Is.EqualTo(1));
        Assert.That(actual.First().UserName, Is.EqualTo("lisa"));
    }
}
