using FirstInteract.Core.Entities;

namespace FirstInteract.Core.Services;

public interface IUserService
{
    ToDoUser RegisterUser(long telegramUserId, string telegramUserName);
    ToDoUser? GetUser(long telegramUserId);
}