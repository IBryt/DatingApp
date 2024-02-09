using NUnit.Framework;
using System.Net;
using Tests.TestHelpers;

namespace Tests.IntegrationControllerTests;

public class AdminIntegrationTests
{
    private CustomWebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private const string RequestUri = "api/admin";
    private string _adminToken;
    private string _moderatorToken;
    private string _memberToken;

    [OneTimeSetUp]
    public async Task Init()
    {
        using var factory = new CustomWebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        _adminToken = await IntegrationTestsHelpers.GetToken("admin", "Pa$$w0rd", factory, client);
        _moderatorToken = await IntegrationTestsHelpers.GetToken("moderator", "Pa$$w0rd", factory, client);
        _memberToken = await IntegrationTestsHelpers.GetToken("lisa", "Pa$$w0rd", factory, client);

    }

    [SetUp]
    public void SetUp()
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
    public async Task GetUsersWithRoles_ReturnsSuccessAndExpectedContent()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, RequestUri + "/users-with-roles");
        request.Headers.Add("Authorization", "Bearer " + _adminToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        var act = await IntegrationTestsHelpers.DeserializeResponse<IEnumerable<dynamic>>(response);

        Assert.That(act, Is.Not.Null);
        Assert.That(act.Count, Is.EqualTo(12));
    }

    [Test]
    public async Task GetUsersWithRoles_ReturnsForbiddenIfIsNotAdmin()
    {
        await ReturnsForbiddenIfIsNotAdmin(_memberToken);
        await ReturnsForbiddenIfIsNotAdmin(_memberToken);
    }


    public async Task ReturnsForbiddenIfIsNotAdmin(string token)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, RequestUri + "/users-with-roles");
        request.Headers.Add("Authorization", "Bearer " + token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task EditRoles_ReturnsRolesIfUserAdmin()
    {
        // Arrange
        var username = "lisa";
        var expectedRoles = new string[] { "Member", "Moderator" };
        var request = new HttpRequestMessage(HttpMethod.Post, RequestUri + "/edit-roles/" + username + "?roles=Moderator,Member");
        request.Headers.Add("Authorization", "Bearer " + _adminToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        var actual = await IntegrationTestsHelpers.DeserializeResponse<IEnumerable<string>>(response);
        Assert.That(actual, Is.EquivalentTo(expectedRoles)); 
    }

    [Test]
    public async Task EditRoles_ReturnsForbiddenIfUserNotAdmin()
    {
        await ReturnsForbiddenIfUserNotAdmin(_moderatorToken);
        await ReturnsForbiddenIfUserNotAdmin(_memberToken);
    }

    private async Task ReturnsForbiddenIfUserNotAdmin(string token)
    {
        // Arrange
        var username = "lisa";
        var request = new HttpRequestMessage(HttpMethod.Post, RequestUri + "/edit-roles/" + username + "?roles=Moderator,Member");
        request.Headers.Add("Authorization", "Bearer " + token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
}
