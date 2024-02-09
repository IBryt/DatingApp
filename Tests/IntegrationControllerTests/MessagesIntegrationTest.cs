using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tests.TestHelpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Tests.IntegrationControllerTests;

public class MessagesIntegrationTest
{
    private CustomWebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private const string RequestUri = "api/messages";
    private string _toddToken;
    private string _lisaToken;

    [OneTimeSetUp]
    public async Task Init()
    {
        using var factory = new CustomWebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        _toddToken = await IntegrationTestsHelpers.GetToken("todd", "Pa$$w0rd", factory, client);
        _lisaToken = await IntegrationTestsHelpers.GetToken("lisa", "Pa$$w0rd", factory, client);
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
    public async Task CreateMessage_ReturnsMessageDto()
    {
        // Arrange
        var expectedContent = "Content";
        var recipientUsername = "todd";
        var senderUsername = "lisa";
        var message = new CreateMessageDto { RecipientUsername = recipientUsername, Content = expectedContent };
        var request = new HttpRequestMessage(HttpMethod.Post, RequestUri);
        request.Content = IntegrationTestsHelpers.StringContent(message);
        request.Headers.Add("Authorization", "Bearer " + _lisaToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        var actual = await IntegrationTestsHelpers.DeserializeResponse<MessageDto>(response);

        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.SenderUsername, Is.EqualTo(senderUsername));
        Assert.That(actual.RecipientUsername, Is.EqualTo(recipientUsername));
        Assert.That(actual.Content, Is.EqualTo(expectedContent));
    }

    [Test]
    public async Task CreateMessage_ReturnsBagRequestThenRecipientEqualSender()
    {
        // Arrange
        var expectedContent = "Content";
        var recipientUsername = "todd";
        var message = new CreateMessageDto { RecipientUsername = recipientUsername, Content = expectedContent };
        var request = new HttpRequestMessage(HttpMethod.Post, RequestUri);
        request.Content = IntegrationTestsHelpers.StringContent(message);
        request.Headers.Add("Authorization", "Bearer " + _toddToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(false));
    }

    [Test]
    public async Task GetMessagesForUser_ReturnsMessages()
    {
        AddMessages();

        await GetMessagesForUser("", 1);
        await GetMessagesForUser("?container=Inbox", 2);
        await GetMessagesForUser("?container=Outbox", 3);
    }

    private async Task GetMessagesForUser(string query, int messageCount)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, RequestUri + query);
        request.Headers.Add("Authorization", "Bearer " + _toddToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        var actual = await IntegrationTestsHelpers.DeserializeResponse<IEnumerable<MemberDto>>(response);
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));
        Assert.That(actual.Count, Is.EqualTo(messageCount));
    }


    private void AddMessages()
    {
        using var scope = _factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        var recipient = context.Users.First(x => x.UserName == "lisa");
        var sender = context.Users.First(x => x.UserName == "todd");

        var messageInbox1 = NewMessage(recipient, sender);
        context.Messages.Add(messageInbox1);

        var messageInbox2 = NewMessage(recipient, sender);
        messageInbox2.DateRead = DateTime.UtcNow;
        context.Messages.Add(messageInbox2);

        var messageOutbox1 = NewMessage(sender, recipient);
        var messageOutbox2 = NewMessage(sender, recipient);
        var messageOutbox3 = NewMessage(sender, recipient);

        context.Messages.Add(messageOutbox1);
        context.Messages.Add(messageOutbox2);
        context.Messages.Add(messageOutbox3);

        context.SaveChanges();
    }

    private Message NewMessage(AppUser sender, AppUser recipient)
    {
        return new Message
        {
            SenderId = sender.Id,
            SenderUsername = sender.UserName,
            Sender = sender,
            RecipientId = recipient.Id,
            RecipientUsername = recipient.UserName,
            Recipient = recipient,
            Content = "Content"
        };
    }

    [Test]
    public async Task DeleteMessage_ShouldDeletedMessageSender()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var message = AddNewMessage(context);
        var request = new HttpRequestMessage(HttpMethod.Delete, RequestUri + "/" + message.Id);
        request.Headers.Add("Authorization", "Bearer " + _toddToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        message = await context.Messages.FindAsync(message.Id);
        Assert.That(message.SenderDeleted, Is.True);
    }

    [Test]
    public async Task DeleteMessage_ShouldDeletedMessageRecipient()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var message = AddNewMessage(context);
        var request = new HttpRequestMessage(HttpMethod.Delete, RequestUri + "/" + message.Id);
        request.Headers.Add("Authorization", "Bearer " + _lisaToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        message = await context.Messages.FindAsync(message.Id);
        Assert.That(message.RecipientDeleted, Is.True);
    }

    [Test]
    public async Task DeleteMessage_ShouldDeletedMessageFromDbIfBothUsersDeletedMessage()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var message = AddNewMessage(context);
        var requestLisa = new HttpRequestMessage(HttpMethod.Delete, RequestUri + "/" + message.Id);
        requestLisa.Headers.Add("Authorization", "Bearer " + _lisaToken);
        var requestTodd = new HttpRequestMessage(HttpMethod.Delete, RequestUri + "/" + message.Id);
        requestTodd.Headers.Add("Authorization", "Bearer " + _toddToken);

        // Act
        await _client.SendAsync(requestLisa);
        await _client.SendAsync(requestTodd);

        // Assert
        message = await context.Messages.FindAsync(message.Id);
        Assert.That(message, Is.Null);
    }

    private Message AddNewMessage(DataContext context)
    {
        var recipient = context.Users.First(x => x.UserName == "lisa");
        var sender = context.Users.First(x => x.UserName == "todd");

        var message = NewMessage(sender, recipient);
        context.Messages.Add(message);
        context.SaveChanges();

        context.Entry(message).State = EntityState.Detached;

        return message;
    }
}