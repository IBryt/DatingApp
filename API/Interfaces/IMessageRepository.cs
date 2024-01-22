using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageRepository
{
    void AddMessage(Message message);
    void DeleteMessage(Message message);
    Task<Message> GetMessageAsync(int id);
    Task<PagedList<Message>> GetMessagesForUserAsync();
    Task<IEnumerable<MessageDto>> GetMessageThreadAsync(int currentUserId, int recipientId);
    Task<bool> SaveAllAsync();
}
