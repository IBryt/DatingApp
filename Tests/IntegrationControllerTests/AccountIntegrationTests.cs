using API.DTOs;
using NUnit.Framework;
using System.Net;
using System.Text;
using System.Text.Json;
using Tests.TestHelpers;


namespace Tests.IntegrationControllerTests;

public class AccountIntegrationTests
{
    private CustomWebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private const string RequestUri = "api/account";

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
    public async Task Redister_ReturnsUserDtoIfValidData()
    {
        //arrange
        var registerDto = new RegisterDto
        {
            Username = "bob1",
            KnownAs = "Bob1",
            Gender = "male",
            DateOfBirth = new DateTime(2000, 12, 1),
            City = "city",
            Country = "country",
            Password = "Pa$$w0rd",
        };
        var content = IntegrationTestsHelpers.StringContent(registerDto);

        // act
        var httpResponse = await _client.PostAsync(RequestUri + "/register", content);

        // assert
       
        var actual = await IntegrationTestsHelpers.DeserializeResponse<UserDto>(httpResponse);

        Assert.That(actual.Username, Is.EqualTo(registerDto.Username));
        Assert.That(actual.KnownAs, Is.EqualTo(registerDto.KnownAs));
        Assert.That(actual.Gender, Is.EqualTo(registerDto.Gender));
        Assert.That(actual.Token, Is.Not.Empty);
    }

    [Test]
    public async Task Redister_ReturnsBadRequestIfPasswordIsEmpty()
    {
        //arrange
        var registerDto = new RegisterDto
        {
            Username = "bob",
            KnownAs = "Bob",
            Gender = "male",
            DateOfBirth = new DateTime(2000, 12, 1, 0, 0, 0, DateTimeKind.Local),
            City = "city",
            Country = "country",
            Password = "",
        };
        var content = IntegrationTestsHelpers.StringContent(registerDto);

        // act
        var httpResponse = await _client.PostAsync(RequestUri + "/register", content);

        // assert
        Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Redister_ReturnsBadRequestIfInvalidDate()
    {
        //arrange
        var registerDto = new RegisterDto
        {
            Username = "bob",
            KnownAs = "Bob",
            Gender = "male",
            DateOfBirth = DateTime.Now,
            City = "city",
            Country = "country",
            Password = "",
        };
        var content = IntegrationTestsHelpers.StringContent(registerDto);

        // act
        var httpResponse = await _client.PostAsync(RequestUri + "/register", content);

        // assert
        Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Redister_ReturnsBadRequestIfPasswordIsInvalid()
    {
        //arrange
        var registerDto = new RegisterDto
        {
            Username = "bob",
            KnownAs = "Bob",
            Gender = "male",
            DateOfBirth = new DateTime(2000, 12, 1, 0, 0, 0, DateTimeKind.Local),
            City = "city",
            Country = "country",
            Password = "12345",
        };
        var content = IntegrationTestsHelpers.StringContent(registerDto);

        // act
        var httpResponse = await _client.PostAsync(RequestUri + "/register", content);

        // assert
        Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }


    [Test]
    public async Task Login_ReturnsUserDtoIfValidCredential()
    {
        //arrange
        var loginDto = new LoginDto
        {
            Username = "lisa",
            Password = "Pa$$w0rd",
        };
        var content = IntegrationTestsHelpers.StringContent(loginDto);

        // act
        var httpResponse = await _client.PostAsync(RequestUri + "/login", content);

        // assert
        var actual = await IntegrationTestsHelpers.DeserializeResponse<UserDto>(httpResponse);
        Assert.That(actual.Username, Is.EqualTo(loginDto.Username));
        Assert.That(actual.KnownAs, Is.Not.Empty);
        Assert.That(actual.Gender, Is.Not.Empty);
        Assert.That(actual.Token, Is.Not.Empty);
    }

    [Test]
    public async Task Login_ReturnsUnauthorizedIfInvalidCredential()
    {
        //arrange
        var loginDto = new LoginDto
        {
            Username = "lisa",
            Password = "",
        };
        var content = IntegrationTestsHelpers.StringContent(loginDto);

        // act
        var httpResponse = await _client.PostAsync(RequestUri + "/login", content);

        // assert
        Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
