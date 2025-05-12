using FirstInteract.Core.DataAccess;
using FirstInteract.Core.Entities;
using FirstInteract.Core.Exceptions;

namespace FirstInteract.Infrastructure.DataAccess;

public class InMemoryToDoRepository : IToDoRepository
{
    private readonly List<ToDoItem> _items = [];

    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
    {
        return _items.Where(x => x.User.UserId == userId).ToList();
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
    {
        return _items.Where(x => x.User.UserId == userId && x.State == ToDoItem.ToDoItemState.Active).ToList();
    }

    public async Task<ToDoItem?> Get(Guid id, CancellationToken ct)
    {
        return _items.FirstOrDefault(x => x.Id == id);
    }

    public async Task Add(ToDoItem item, CancellationToken ct)
    {
        _items.Add(item);
    }

    public async Task Update(ToDoItem item, CancellationToken ct)
    {
        var index = _items.FindIndex(x => x.Id == item.Id);
        _items[index] = item;
    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        _items.RemoveAll(x => x.Id == id);
    }

    public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
    {
        return _items.FirstOrDefault(x => x.User.UserId == userId && x.Name == name) != null;
    }

    public async Task<int> CountActive(Guid userId, CancellationToken ct)
    {
        return _items.Count(x => x.User.UserId == userId);
    }

    public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
    {
        return _items.Where(x => x.User.UserId == userId && predicate(x)).ToList();
    }
}