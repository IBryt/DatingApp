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
    private readonly PresenceTracker _presenceTracker;

    public MessageHub(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        PresenceTracker presenceTracker)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _presenceTracker = presenceTracker;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext?.Request.Query["user"].ToString();
        if (otherUser == null)
        {
            throw new HubException("otherUser cannot be null");
        }
        var callerUser = Context.User.GetUsername();

        var groupName = GetGroupName(callerUser, otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        await Clients.Group(groupName).SendAsync("UpdatedGroup");

        var messages = await _unitOfWork.MessageRepository.GetMessageThreadAsync(callerUser, otherUser);

        if (_unitOfWork.HasChanges())
        {
            await _unitOfWork.Complete();
        }

        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
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

        if (sender == null)
        {
            throw new HubException("Not found sender");
        }

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

        if (await _presenceTracker.IsOnlineAsync(recipientUsername))
        {
            message.DateRead = DateTime.UtcNow;
        }

        _unitOfWork.MessageRepository.AddMessage(message);
        if (!await _unitOfWork.Complete())
        {
            throw new HubException("Failed to sent message");
        }

        var messageDto = _mapper.Map<MessageDto>(message);
        await Clients.Group(groupName).SendAsync("NewMessage", messageDto);
    }

    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
}
