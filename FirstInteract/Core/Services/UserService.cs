using FirstInteract.Core.DataAccess;
using FirstInteract.Core.Entities;

namespace FirstInteract.Core.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    // private readonly List<ToDoUser> _users = [];
    // private IUserRepository _userRepository = userRepository;
    
    public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
    {
        var newUser = new ToDoUser()
        {
            RegisteredAt = DateTime.Now,
            TelegramUserId = telegramUserId,
            TelegramUserName = telegramUserName,
            UserId = Guid.NewGuid()
        };
        
        userRepository.Add(newUser);
        return newUser;
    }

    public ToDoUser? GetUser(long telegramUserId)
    {
        return userRepository.GetUserByTelegramUserId(telegramUserId);
    }
}