namespace FirstInteract.TelegramBot.Dto;

public class ToDoListCallbackDto(string action, string? toDoListId) : CallbackDto(action)
{
    public Guid? ToDoListId { get; set; }
    
    // На вход принимает строку вида "{action}|{toDoListId}|{prop2}...".
    // Нужно создать ToDoListCallbackDto с Action = action и ToDoListId = toDoListId.
    public new static ToDoListCallbackDto FromString(string input)
    {
        var action = input.Split("|")[0];
        var toDoListId = input.Split("|")[1];
        var cbDto = new ToDoListCallbackDto(action, toDoListId);
        return cbDto;
    }
    
    public override string ToString()
    {
        return $"{base.ToString()}|{ToDoListId}";
    }
}