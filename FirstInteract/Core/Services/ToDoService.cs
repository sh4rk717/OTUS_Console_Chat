using FirstInteract.Core.DataAccess;
using FirstInteract.Core.Entities;
using FirstInteract.Core.Exceptions;

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
        var newTask = Program.ValidateString(name);

        // проверка на кол-во задач
        if (repository.GetAllByUserId(user.UserId).Count >= Program.MaxTasks)
            throw new TaskCountLimitException(Program.MaxTasks);
        
        // проверка на длину имени задачи
        if (newTask.Length > Program.MaxTaskLength)
            throw new TaskLengthLimitException(newTask.Length, Program.MaxTaskLength);

        // проверка на дубликат задачи по имени задачи
        if (repository.GetActiveByUserId(user.UserId).FirstOrDefault(x=>x.Name == name) != null)
            throw new DuplicateTaskException(newTask);
        
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