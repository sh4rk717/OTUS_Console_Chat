namespace FirstInteract.TelegramBot.Dto;

public class ToDoListCallbackDto(string action, Guid? toDoListId) : CallbackDto(action)
{
    public Guid? ToDoListId { get; } = toDoListId;

    public new static ToDoListCallbackDto FromString(string input)
    {
        var action = input.Split("|")[0];
        var toDoListId = input.Split("|")[1];
        if (!Guid.TryParse(toDoListId, out var toDoListGuid)) return new ToDoListCallbackDto(action, null);
        var cbDto = new ToDoListCallbackDto(action, toDoListGuid);
        return cbDto;
    }

    public override string ToString()
    {
        return $"{base.ToString()}|{ToDoListId}";
    }
}