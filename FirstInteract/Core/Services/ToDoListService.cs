using FirstInteract.Core.DataAccess;
using FirstInteract.Core.Entities;
using FirstInteract.Core.Exceptions;

namespace FirstInteract.Core.Services;

public class ToDoListService(IToDoListRepository repository) : IToDoListService
{
    public async Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct)
    {
        var newList = new ToDoList(user, name);
        var newListName = Program.ValidateString(name);
        
        // проверка на длину имени списка
        if (newListName.Length > 10)
            throw new TaskLengthLimitException(newListName.Length, 10);

        // проверка на дубликат списка по имени списка
        if (await repository.ExistsByName(user.UserId, name, ct))
            throw new DuplicateTaskException(newListName);
        
        await repository.Add(newList, ct);

        return newList;
    }

    public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
    {
        return await repository.Get(id, ct);
    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        await repository.Delete(id, ct);
    }

    public async Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct)
    {
        return await repository.GetByUserId(userId, ct);
    }
}