using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

public class MessageHub : Hub
{
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IHubContext<PresenceHub> _presenceHub;
    private readonly PresenceTracker _presenceTracker;

    public MessageHub(
        IMessageRepository messageRepository,
        IMapper mapper,
        IUserRepository userRepository,
        IHubContext<PresenceHub> presenceHub,
        PresenceTracker presenceTracker)
    {
        _messageRepository = messageRepository;
        _mapper = mapper;
        _userRepository = userRepository;
        _presenceHub = presenceHub;
        _presenceTracker = presenceTracker;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext.Request.Query["user"].ToString();
        var callerUser = Context.User.GetUsername();

        var groupName = GetGroupName(callerUser, otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await AddToGroupAsync(Context, groupName);

        var messages = await _messageRepository.GetMessageThreadAsync(callerUser, otherUser);

        await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await RemoveFromMessageGroupAsync();
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var username = Context.User.GetUsername();
        var recipientUsername = createMessageDto.RecipientUsername.ToLower();

        if (username == recipientUsername)
        {
            throw new HubException("You cannot send messages to yourself");
        }

        var sender = await _userRepository.GetUserByUsernameAsync(username);
        var recipient = await _userRepository.GetUserByUsernameAsync(recipientUsername);

        if (recipient == null)
        {
            throw new HubException("Not found user");
        }

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        var groupName = GetGroupName(sender.UserName, recipient.UserName);

        var group = await _messageRepository.GetGroupAsync(groupName);
        if (group.Connections.Any(x => x.Username == recipientUsername))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            var connections = await _presenceTracker.GetConnectionsForUserAsync(recipientUsername);
            if (connections != null)
            {
                await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                    new { username = sender.UserName, knownAs = sender.KnownAs });
            }
        }

        _messageRepository.AddMessage(message);
        if (!await _messageRepository.SaveAllAsync())
        {
            throw new HubException("Failed to sent message");
        }

        var messageDto = _mapper.Map<MessageDto>(message);
        await Clients.Group(groupName).SendAsync("NewMessage", messageDto);
    }

    private async Task<bool> AddToGroupAsync(HubCallerContext context, string groupName)
    {
        var group = await _messageRepository.GetGroupAsync(groupName);
        var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

        if (group == null)
        {
            group = new Group(groupName);
            _messageRepository.AddGroup(group);
        }

        group.Connections.Add(connection);

        return await _messageRepository.SaveAllAsync();
    }

    private async Task RemoveFromMessageGroupAsync()
    {
        var connection = await _messageRepository.GetConnectionAsync(Context.ConnectionId);
        _messageRepository.RemoveConnection(connection);
        await _messageRepository.SaveAllAsync();
    }

    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
}
