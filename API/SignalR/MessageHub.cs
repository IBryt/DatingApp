using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

public class MessageHub : Hub
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHubContext<PresenceHub> _presenceHub;
    private readonly PresenceTracker _presenceTracker;

    public MessageHub(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IHubContext<PresenceHub> presenceHub,
        PresenceTracker presenceTracker)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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
        var group = await AddToGroupAsync(groupName);

        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await _unitOfWork.MessageRepository.GetMessageThreadAsync(callerUser, otherUser);

        if (_unitOfWork.HasChanges())
        {
            await _unitOfWork.Complete();
        }
        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var group = await RemoveFromMessageGroupAsync();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
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

        var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
        var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(recipientUsername);

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

        var group = await _unitOfWork.MessageRepository.GetGroupAsync(groupName);
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

        _unitOfWork.MessageRepository.AddMessage(message);
        if (!await _unitOfWork.Complete())
        {
            throw new HubException("Failed to sent message");
        }

        var messageDto = _mapper.Map<MessageDto>(message);
        await Clients.Group(groupName).SendAsync("NewMessage", messageDto);
    }

    private async Task<Group> AddToGroupAsync(string groupName)
    {
        var group = await _unitOfWork.MessageRepository.GetGroupAsync(groupName);
        var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

        if (group == null)
        {
            group = new Group(groupName);
            _unitOfWork.MessageRepository.AddGroup(group);
        }

        group.Connections.Add(connection);

        if (!await _unitOfWork.Complete())
        {
            throw new HubException("Failed to join group");
        }

        return group;
    }

    private async Task<Group> RemoveFromMessageGroupAsync()
    {
        var group = await _unitOfWork.MessageRepository.GetGroupForConnectionAsync(Context.ConnectionId);
        var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
        _unitOfWork.MessageRepository.RemoveConnection(connection);

        if (!await _unitOfWork.Complete())
        {
            throw new HubException("Failed to remove from group");
        }

        return group;
    }

    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
}
