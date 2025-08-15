namespace FirstInteract.TelegramBot.Dto;

public class ToDoItemCallbackDto(string action, Guid toDoItemId) : CallbackDto(action)
{
    private Guid ToDoItemId { get; } = toDoItemId;

    public new static ToDoItemCallbackDto FromString(string input)
    {
        var action = input.Split("|")[0];
        var toDoItemId = input.Split("|")[1];
        if (!Guid.TryParse(toDoItemId, out var toDoItemGuid)) return new ToDoItemCallbackDto(action, Guid.Empty);
        var cbDto = new ToDoItemCallbackDto(action, toDoItemGuid);
        return cbDto;
    }

    public override string ToString()
    {
        return $"{base.ToString()}|{ToDoItemId}";
    }
}