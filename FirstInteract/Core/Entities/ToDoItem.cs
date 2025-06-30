namespace FirstInteract.Core.Entities;

public class ToDoItem
{
    public enum ToDoItemState
    {
        Active,
        Completed
    }

    public Guid Id { get; set; } = Guid.NewGuid();
    public ToDoUser User { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime Deadline { get; set; }
    public ToDoItemState State { get; set; } = ToDoItemState.Active;
    public DateTime? StateChangedAt { get; set; } = DateTime.Now;

    public ToDoItem(string name, ToDoUser user, DateTime deadline)
    {
        User = user;   
        Name = name;
        Deadline = deadline;
    }
}