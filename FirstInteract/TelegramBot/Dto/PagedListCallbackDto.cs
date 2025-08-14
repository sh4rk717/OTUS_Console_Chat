namespace FirstInteract.TelegramBot.Dto;

public class PagedListCallbackDto(string action, Guid? toDoListId, int page) : ToDoListCallbackDto(action, toDoListId)
{
    public int Page { get; } = page;

    public new static PagedListCallbackDto FromString(string input)
    {
        var action = input.Split("|")[0];
        var toDoListId = input.Split("|")[1];
        var isIntPage = int.TryParse(input.Split("|")[2], out var page);
        if (!isIntPage) page = 0;

        return !Guid.TryParse(toDoListId, out var toDoListGuid)
            ? new PagedListCallbackDto(action, null, page)
            : new PagedListCallbackDto(action, toDoListGuid, page);
    }

    public override string ToString()
    {
        return $"{base.ToString()}|{Page}";
    }
}