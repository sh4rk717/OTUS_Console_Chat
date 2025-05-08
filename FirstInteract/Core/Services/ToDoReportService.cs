using FirstInteract.Core.DataAccess;
using FirstInteract.Core.Entities;

namespace FirstInteract.Core.Services;

public class ToDoReportService(IToDoRepository toDoRepository) : IToDoReportService
{
    public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
    {
        // userRepository.GetUser(userId);
        var total = toDoRepository.GetAllByUserId(userId).Count();
        var completed = toDoRepository.GetAllByUserId(userId).Count(x => x.State == ToDoItem.ToDoItemState.Completed);
        var active = toDoRepository.GetActiveByUserId(userId).Count();

        return (total, completed, active, generatedAt: DateTime.Now);
    }
}