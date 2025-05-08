using FirstInteract.Core.DataAccess;
using FirstInteract.Core.Entities;

namespace FirstInteract.Core.Services;

public class ToDoService(IToDoRepository repository) : IToDoService
{
    // private readonly List<ToDoItem> _items = [];
    // private IToDoRepository _repository = repository;

    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        return repository.GetAllByUserId(userId);
    }

    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        return repository.GetActiveByUserId(userId);
    }

    public ToDoItem Add(ToDoUser user, string name)
    {
        var newTaskItem = new ToDoItem(name, user);
        repository.Add(newTaskItem);

        return newTaskItem;
    }

    public void MarkCompleted(Guid id)
    {
        var task = repository.Get(id);
        if (task == null) return;
        task.State = ToDoItem.ToDoItemState.Completed;
        task.StateChangedAt = DateTime.Now;
        repository.Update(task);
    }

    public void Delete(Guid id)
    {
        repository.Delete(id);
    }

    public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
    {
        return repository.Find(user.UserId, item => item.Name.StartsWith(namePrefix));
    }
}