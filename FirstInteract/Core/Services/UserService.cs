using FirstInteract.Core.DataAccess;
using FirstInteract.Core.Entities;

namespace FirstInteract.Core.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<ToDoUser> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken ct)
    {
        var newUser = new ToDoUser()
        {
            RegisteredAt = DateTime.Now,
            TelegramUserId = telegramUserId,
            TelegramUserName = telegramUserName,
            UserId = Guid.NewGuid()
        };
        
        await userRepository.Add(newUser, ct);
        return newUser;
    }

    public async Task<ToDoUser?> GetUser(long telegramUserId, CancellationToken ct)
    {
        return await userRepository.GetUserByTelegramUserId(telegramUserId, ct);
    }
}