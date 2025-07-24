namespace FirstInteract.Core.Entities;

public class ToDoList(ToDoUser user, string name)
{
    /*
     Добавить класс ToDoList по пути Core/Entities
       Свойства
       Guid Id
       string Name
       ToDoUser User
       DateTime CreatedAt
       Добавить свойство ToDoList? List в ToDoItem
       Добавить аргумент ToDoList? list в IToDoService.Add
     */
    
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public ToDoUser User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}