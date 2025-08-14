namespace FirstInteract.Core.Entities;

public class ToDoList(ToDoUser user, string name)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = name;
    public ToDoUser User { get; init; } = user;
    public DateTime CreatedAt { get; init; } = DateTime.Now;
}