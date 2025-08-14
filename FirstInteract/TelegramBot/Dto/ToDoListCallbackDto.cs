namespace FirstInteract.TelegramBot.Dto;

public class ToDoListCallbackDto(string action, Guid? toDoListId) : CallbackDto(action)
{
    public new string Action { get; set; } = action;

    public Guid? ToDoListId { get; } = toDoListId;

    public new static ToDoListCallbackDto FromString(string input)
    {
        var action = input.Split("|")[0];
        var toDoListId = input.Split("|")[1];
        if (Guid.TryParse(toDoListId, out var toDoListGuid))
        {
            var cbDto = new ToDoListCallbackDto(action, toDoListGuid);
            return cbDto;
        }
        return new ToDoListCallbackDto(action, null);
    }
    
    public override string ToString()
    {
        return $"{base.ToString()}|{ToDoListId}";
    }
}