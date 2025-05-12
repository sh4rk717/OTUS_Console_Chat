using FirstInteract.Core.DataAccess;
using FirstInteract.Core.Entities;
using FirstInteract.Core.Exceptions;

namespace FirstInteract.Core.Services;

public class ToDoService(IToDoRepository repository) : IToDoService
{
    // private readonly List<ToDoItem> _items = [];
    // private IToDoRepository _repository = repository;

    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
    {
        return await repository.GetAllByUserId(userId, ct);
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
    {
        return await repository.GetActiveByUserId(userId, ct);
    }

    public async Task<ToDoItem> Add(ToDoUser user, string name, CancellationToken ct)
    {
        var newTaskItem = new ToDoItem(name, user);
        var newTask = Program.ValidateString(name);

        // проверка на кол-во задач
        if (await repository.CountActive(user.UserId, ct) >= Program.MaxTasks)
            throw new TaskCountLimitException(Program.MaxTasks);
        
        // проверка на длину имени задачи
        if (newTask.Length > Program.MaxTaskLength)
            throw new TaskLengthLimitException(newTask.Length, Program.MaxTaskLength);

        // проверка на дубликат задачи по имени задачи
        if (await repository.ExistsByName(user.UserId, name, ct))
            throw new DuplicateTaskException(newTask);
        
        await repository.Add(newTaskItem, ct);

        return newTaskItem;
    }

    public async Task MarkCompleted(Guid id, CancellationToken ct)
    {
        var item = await repository.Get(id, ct);
        if (item == null) return;
        item.State = ToDoItem.ToDoItemState.Completed;
        item.StateChangedAt = DateTime.Now;
        await repository.Update(item, ct);
    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        await repository.Delete(id, ct);
    }

    public async Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken ct)
    {
        return await repository.Find(user.UserId, item => item.Name.StartsWith(namePrefix), ct);
    }
}