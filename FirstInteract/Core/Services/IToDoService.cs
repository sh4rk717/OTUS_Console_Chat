using FirstInteract.Core.Entities;

namespace FirstInteract.Core.Services;

public interface IToDoService
{
    Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct);
    //Возвращает ToDoItem для UserId со статусом Active
    Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct);
    Task<ToDoItem> Add(ToDoUser user, string name, CancellationToken ct);
    Task MarkCompleted(Guid id, CancellationToken ct);
    Task Delete(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken ct);
}