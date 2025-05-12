using FirstInteract.Core.Entities;

namespace FirstInteract.Core.DataAccess;

public interface IToDoRepository
{
    Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct);
    //Возвращает ToDoItem для UserId со статусом Active
    Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct);
    Task<ToDoItem?> Get(Guid id, CancellationToken ct);
    Task Add(ToDoItem item, CancellationToken ct);
    Task Update(ToDoItem item, CancellationToken ct);
    Task Delete(Guid id, CancellationToken ct);
    //Проверяет есть ли задача с таким именем у пользователя
    Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct);
    //Возвращает количество активных задач у пользователя
    Task<int> CountActive(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct);
}