using API.Data;
using API.Data.Repositories;
using API.Entities;
using API.Helpers;
using API.Interfaces.Repositories;
using NUnit.Framework;
using Tests.TestHelpers;

namespace Tests.Data;

public class MessageRepositoryTests
{
    private DataContext _context;
    private IMessageRepository _messageRepository;
    private AppUser _sender;
    private AppUser _recipient;
    private Message _message;

    [SetUp]
    public void SetUp()
    {
        _context = DataHelpers.GetDatabase();
        DataHelpers.AddUsers(_context);
        _messageRepository = new MessageRepository(_context, DataHelpers.GetMapperProfile());
        _sender = _context.Users.Find(1);
        _recipient = _context.Users.Find(2);
        _message = NewMessage(_sender, _recipient);
    }

    [Test]
    public async Task AddMessage_ShouldReturnMessageWithId()
    {
        // Act
        await _messageRepository.AddMessageAsync(_message);
        _context.SaveChanges();

        // Assert
        Assert.That(_message.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task DeleteMessage_ShouldRemoveMessage()
    {
        // Arrange
        await _messageRepository.AddMessageAsync(_message);
        _context.SaveChanges();

        // Act
        _messageRepository.DeleteMessage(_message);
        _context.SaveChanges();

        // Assert
        Assert.That(_context.Messages.Count(), Is.EqualTo(0));
    }

    [TestCase(1, 1)]
    public async Task GetMessageAsync_ShouldReturnMessageByIdIfExist(int id, int expectedId)
    {
        // Arrange
        await _messageRepository.AddMessageAsync(_message);
        _context.SaveChanges();

        // Act
        var message = await _messageRepository.GetMessageAsync(id);

        // Assert
        Assert.That(message.Id, Is.EqualTo(expectedId));
    }

    [Test]
    public async Task GetMessageAsync_ShouldReturnMessageByIdIfNotExist()
    {
        // Arrange
        int id = 2;
        await _messageRepository.AddMessageAsync(_message);
        _context.SaveChanges();

        // Act
        var message = await _messageRepository.GetMessageAsync(id);

        // Assert
        Assert.That(message, Is.EqualTo(null));
    }

    [TestCase("Unread", 1)]
    [TestCase("Inbox", 2)]
    [TestCase("Outbox", 3)]
    public async Task GetMessagesForUserAsync_ShouldReturnMessageByIdIfNotExist(string container, int count)
    {
        // Arrange
        AddMessages();
        var messageParams = new MessageParams { Username = _sender.UserName, Container = container };

        // Act
        var messages = await _messageRepository.GetMessagesForUserAsync(messageParams);

        // Assert
        Assert.That(messages.Count, Is.EqualTo(count));
    }

    [Test]
    public async Task GetMessageThreadAsync_ShouldReturnMessageByIdIfNotExist()
    {
        // Arrange
        AddMessages();

        // Act
        var messages = await _messageRepository.GetMessageThreadAsync(_sender.UserName, _recipient.UserName);

        // Assert
        Assert.That(messages.Count, Is.EqualTo(5));
    }

    private void AddMessages()
    {
        var messageUnread = NewMessage(_recipient, _sender);
        _context.Messages.Add(messageUnread);

        var messageInbox1 = NewMessage(_recipient, _sender);
        messageInbox1.DateRead = DateTime.UtcNow;
        _context.Messages.Add(messageInbox1);

        var messageOutbox1 = NewMessage(_sender, _recipient);
        var messageOutbox2 = NewMessage(_sender, _recipient);
        var messageOutbox3 = NewMessage(_sender, _recipient);

        _context.Messages.Add(messageOutbox1);
        _context.Messages.Add(messageOutbox2);
        _context.Messages.Add(messageOutbox3);

        _context.SaveChanges();
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
}
