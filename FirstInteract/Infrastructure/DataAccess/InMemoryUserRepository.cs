using FirstInteract.Core.DataAccess;
using FirstInteract.Core.Entities;

namespace FirstInteract.Infrastructure.DataAccess;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<ToDoUser> _users = [];
    
    public Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct)
    {
        return Task.FromResult(_users.FirstOrDefault(user => user.UserId == userId));
    }

    public Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
    {
        return Task.FromResult(_users.FirstOrDefault(user => user.TelegramUserId == telegramUserId));
    }

    public Task Add(ToDoUser user, CancellationToken ct)
    {
        if (!_users.Contains(user))
        {
            _users.Add(user);
        }

        return Task.CompletedTask;
    }
}