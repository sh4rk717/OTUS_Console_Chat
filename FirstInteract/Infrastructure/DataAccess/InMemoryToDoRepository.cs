using FirstInteract.Core.DataAccess;
using FirstInteract.Core.Entities;
using FirstInteract.Core.Exceptions;

namespace FirstInteract.Infrastructure.DataAccess;

public class InMemoryToDoRepository : IToDoRepository
{
    private readonly List<ToDoItem> _items = [];

    public Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
    {
        return Task.FromResult<IReadOnlyList<ToDoItem>>(_items.Where(x => x.User.UserId == userId).ToList());
    }

    public Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
    {
        return Task.FromResult<IReadOnlyList<ToDoItem>>(_items.Where(x => x.User.UserId == userId && x.State == ToDoItem.ToDoItemState.Active).ToList());
    }

    public Task<ToDoItem?> Get(Guid id, CancellationToken ct)
    {
        return Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
    }

    public Task Add(ToDoItem item, CancellationToken ct)
    {
        _items.Add(item);
        return Task.CompletedTask;
    }

    public Task Update(ToDoItem item, CancellationToken ct)
    {
        var index = _items.FindIndex(x => x.Id == item.Id);
        _items[index] = item;
        return Task.CompletedTask;
    }

    public Task Delete(Guid id, CancellationToken ct)
    {
        _items.RemoveAll(x => x.Id == id);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
    {
        return Task.FromResult(_items.Any(x => x.User.UserId == userId && x.Name == name));
    }

    public Task<int> CountActive(Guid userId, CancellationToken ct)
    {
        return Task.FromResult(_items.Count(x => x.User.UserId == userId));
    }

    public Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
    {
        return Task.FromResult<IReadOnlyList<ToDoItem>>(_items.Where(x => x.User.UserId == userId && predicate(x)).ToList());
    }
}