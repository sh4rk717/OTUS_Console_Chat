using FirstInteract.Core.DataAccess;
using FirstInteract.Core.Entities;

namespace FirstInteract.Infrastructure.DataAccess;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<ToDoUser> _users = [];
    
    public ToDoUser? GetUser(Guid userId)
    {
        return _users.FirstOrDefault(user => user.UserId == userId);
    }

    public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
    {
        return _users.FirstOrDefault(user => user.TelegramUserId == telegramUserId);
    }

    public void Add(ToDoUser user)
    {
        if (!_users.Contains(user))
        {
            _users.Add(user);
        }
    }
}