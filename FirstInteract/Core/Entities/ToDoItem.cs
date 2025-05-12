namespace FirstInteract.Core.Entities;

public class ToDoItem(string name, ToDoUser user)
{
    public enum ToDoItemState
    {
        Active,
        Completed
    }

    public Guid Id { get; set; } = Guid.NewGuid();
    public ToDoUser User { get; set; } = user;
    public string Name { get; set; } = name;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public ToDoItemState State { get; set; } = ToDoItemState.Active;
    public DateTime? StateChangedAt { get; set; } = DateTime.Now;
}