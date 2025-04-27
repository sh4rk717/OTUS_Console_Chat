namespace FirstInteract;

public class ToDoUser
{
    public Guid UserId { set; get; }
    public long TelegramUserId { set; get; }
    public string TelegramUserName { set; get; }
    public DateTime RegisteredAt { set; get; }
}