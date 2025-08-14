namespace FirstInteract.Core.Entities;

public class ToDoUser
{
    public Guid UserId { init; get; }
    public long TelegramUserId { init; get; }
    public string? TelegramUserName { init; get; }
    public DateTime RegisteredAt { init; get; }
}