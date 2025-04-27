namespace FirstInteract;

public class ToDoService: IToDoService
{
    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        return Db.TasksList.Where(x => x.User.UserId == userId).ToList();
    }

    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        return Db.TasksList.Where(x => x.User.UserId == userId && x.State == ToDoItem.ToDoItemState.Active).ToList();
    }

    public ToDoItem Add(ToDoUser user, string name)
    {
        if (Db.TasksList.Count >= Program.MaxTasks)
            throw new TaskCountLimitException(Program.MaxTasks);

        var newTask = Program.ValidateString(name);

        if (newTask.Length > Program.MaxTaskLength)
            throw new TaskLengthLimitException(newTask.Length, Program.MaxTaskLength);

        var newTaskItem = new ToDoItem(newTask, user);
        if (Db.TasksList.Contains(newTaskItem))
            throw new DuplicateTaskException(newTask);

        Db.TasksList.Add(newTaskItem);
        
        return newTaskItem;
    }

    public void MarkCompleted(Guid id)
    {
        foreach (var task in Db.TasksList.Where(x => x.Id == id))
        {
            task.State = ToDoItem.ToDoItemState.Completed;
        }
    }

    public void Delete(Guid id)
    {
        Db.TasksList.RemoveAll(x => x.Id == id);
    }
}