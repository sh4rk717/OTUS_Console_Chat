using FirstInteract.Core.DataAccess;
using FirstInteract.Core.Entities;

namespace FirstInteract.Core.Services;

public class ToDoReportService(IToDoRepository toDoRepository) : IToDoReportService
{
    public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStats(Guid userId, CancellationToken ct)
    {
        var totalList = await toDoRepository.GetAllByUserId(userId, ct);
        var completedList = await toDoRepository.GetAllByUserId(userId, ct);
        var activeList = await toDoRepository.GetActiveByUserId(userId, ct);

        var total = totalList.Count;
        var completed = completedList.Count(x => x.State == ToDoItem.ToDoItemState.Completed);
        var active = activeList.Count;

        return (total, completed, active, generatedAt: DateTime.Now);
    }
}