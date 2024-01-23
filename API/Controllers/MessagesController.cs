using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MessagesController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _messageRepository = messageRepository;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var username = User.GetUsername();
        var recipientUsername = createMessageDto.RecipientUsername.ToLower();

        if (username == recipientUsername)
        {
            return BadRequest("You cannot send messages to yourself");
        }

        var sender = await _userRepository.GetUserByUsernameAsync(username);
        var recipient = await _userRepository.GetUserByUsernameAsync(recipientUsername);

        if (recipient == null)
        {
            return NotFound();
        }

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        _messageRepository.AddMessage(message);
        if (!await _messageRepository.SaveAllAsync())
        {
            return BadRequest("Failed to sent message");
        }

        var messageDto = _mapper.Map<MessageDto>(message);
        return Ok(messageDto);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        messageParams.Username = User.GetUsername();

        var messages = await _messageRepository.GetMessagesForUserAsync(messageParams);

        Response.AddPaginationHeeader(
            messages.CurrentPage,
            messages.PageSize,
            messages.TotalCount,
            messages.TotalPages);

        return Ok(messages);
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesThread(string username)
    { 
        var currentUsername = User.GetUsername();
        var messages = await _messageRepository.GetMessageThreadAsync(currentUsername, username);
        return Ok(messages);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id) 
    {
        var username = User.GetUsername();
        var message = await _messageRepository.GetMessageAsync(id);

        if (message.Sender.UserName != username && message.Recipient.UserName != username) 
        {
            return Unauthorized();
        }

        if (message.Sender.UserName == username)
        { 
            message.SenderDeleted = true;
        }

        if (message.Recipient.UserName == username)
        {
            message.RecipientDeleted = true;
        }

        if (message.SenderDeleted && message.RecipientDeleted)
        {
            _messageRepository.DeleteMessage(message);
        }

        if (!await _messageRepository.SaveAllAsync()) 
        { 
            return BadRequest("Problem deleting the message");
        }

        return Ok();
    }
}
