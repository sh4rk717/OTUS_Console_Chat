using FirstInteract.Core.Entities;

namespace FirstInteract.Core.DataAccess;

public interface IUserRepository
{
    ToDoUser? GetUser(Guid userId);
    ToDoUser? GetUserByTelegramUserId(long telegramUserId);
    void Add(ToDoUser user);
}