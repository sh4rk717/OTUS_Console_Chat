namespace FirstInteract;

public class UserService : IUserService
{
    private readonly List<ToDoUser> _users = [];
    private readonly List<string> _commandsList = ["/start", "/help", "/info"];

    public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
    {
        // throw new NotImplementedException();
        foreach (var user in _users.Where(user => user.TelegramUserId == telegramUserId))
        {
            //пользователь уже зарегистрирован
            return user;
        }

        var newUser = new ToDoUser()
        {
            RegisteredAt = DateTime.Now,
            TelegramUserId = telegramUserId,
            TelegramUserName = telegramUserName,
            UserId = Guid.NewGuid()
        };
        _users.Add(newUser);
        _commandsList.AddRange(["/addtask", "/showtasks", "/removetask", "/completetask", "/showalltasks"]);
        return newUser;
    }

    public ToDoUser? GetUser(long telegramUserId)
    {
        return _users.FirstOrDefault(user => user.TelegramUserId == telegramUserId);
    }
}