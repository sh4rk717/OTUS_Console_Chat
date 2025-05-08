namespace FirstInteract;

public class ToDoService : IToDoService
{
    private readonly List<ToDoItem> _items = [];

    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        return _items.Where(x => x.User.UserId == userId).ToList();
    }

    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        return _items.Where(x => x.User.UserId == userId && x.State == ToDoItem.ToDoItemState.Active).ToList();
    }

    public ToDoItem Add(ToDoUser user, string name)
    {
        if (_items.Count >= Program.MaxTasks)
            throw new TaskCountLimitException(Program.MaxTasks);

        var newTask = Program.ValidateString(name);

        if (newTask.Length > Program.MaxTaskLength)
            throw new TaskLengthLimitException(newTask.Length, Program.MaxTaskLength);

        var newTaskItem = new ToDoItem(newTask, user);
        if (_items.Contains(newTaskItem))
            throw new DuplicateTaskException(newTask);

        _items.Add(newTaskItem);

        return newTaskItem;
    }

    public void MarkCompleted(Guid id)
    {
        foreach (var task in _items.Where(x => x.Id == id))
        {
            task.State = ToDoItem.ToDoItemState.Completed;
        }
    }

    public void Delete(Guid id)
    {
        _items.RemoveAll(x => x.Id == id);
    }
}