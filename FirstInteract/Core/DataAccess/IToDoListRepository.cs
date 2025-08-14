using FirstInteract.Core.Entities;

namespace FirstInteract.Core.DataAccess;

public interface IToDoListRepository
{
    //Если списка нет, то возвращает null
    Task<ToDoList?> Get(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct);
    Task Add(ToDoList list, CancellationToken ct);
    Task Delete(Guid id, CancellationToken ct);
    //Проверяет, если ли у пользователя список с таким именем
    Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct);
}