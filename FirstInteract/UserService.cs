namespace FirstInteract;

public class UserService : IUserService
{
    public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
    {
        // throw new NotImplementedException();
        foreach (var user in Db.UserList.Where(user => user.TelegramUserId == telegramUserId))
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
        Db.UserList.Add(newUser);
        Db.CommandsList.AddRange(["/addtask", "/showtasks", "/removetask", "/completetask", "/showalltasks"]);
        return newUser;
    }

    public ToDoUser? GetUser(long telegramUserId)
    {
        return Db.UserList.FirstOrDefault(user => user.TelegramUserId == telegramUserId);
    }
}