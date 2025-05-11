using FirstInteract.Core.DataAccess;
using FirstInteract.Core.Entities;
using FirstInteract.Core.Exceptions;

namespace FirstInteract.Infrastructure.DataAccess;

public class InMemoryToDoRepository : IToDoRepository
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

    public ToDoItem? Get(Guid id)
    {
        return _items.FirstOrDefault(x => x.Id == id);
    }

    public void Add(ToDoItem item)
    {
        _items.Add(item);
    }

    public void Update(ToDoItem item)
    {
        var index = _items.FindIndex(x => x.Id == item.Id);
        _items[index] = item;
    }

    public void Delete(Guid id)
    {
        _items.RemoveAll(x => x.Id == id);
    }

    public bool ExistsByName(Guid userId, string name)
    {
        return _items.FirstOrDefault(x => x.User.UserId == userId && x.Name == name) != null;
    }

    public int CountActive(Guid userId)
    {
        return _items.Count(x => x.User.UserId == userId);
    }

    public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
    {
        return _items.Where(x => x.User.UserId == userId && predicate(x)).ToList();
    }
}