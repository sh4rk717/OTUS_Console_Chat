namespace FirstInteract.Core.Entities;

public class ToDoItem(string name, ToDoUser user, DateTime deadline, ToDoList? list)
{
    public enum ToDoItemState
    {
        Active,
        Completed
    }

    public Guid Id { get; init; } = Guid.NewGuid();
    public ToDoUser User { get; init; } = user;
    public string Name { get; init; } = name;
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime Deadline { get; init; } = deadline;
    public ToDoItemState State { get; set; } = ToDoItemState.Active;
    public DateTime? StateChangedAt { get; set; } = DateTime.Now;
    public ToDoList? List { get; init; } = list;
}