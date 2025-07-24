using FirstInteract.Core.Entities;

namespace FirstInteract.Core.Services;

public interface IToDoListService
{
    Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct);
    Task<ToDoList?> Get(Guid id, CancellationToken ct);
    Task Delete(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct);
}