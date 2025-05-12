namespace FirstInteract.Core.Services;

public interface IToDoReportService
{
    (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId);
}