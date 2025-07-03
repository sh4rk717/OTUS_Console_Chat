namespace FirstInteract.TelegramBot.Scenarios;

/// <summary>
/// Репозиторий, который отвечает за доступ к контекстам пользователей
/// </summary>
public interface IScenarioContextRepository
{
    /// <summary>
    /// Получить контекст пользователя
    /// </summary>
    /// <param name="userId">Id пользователя в Telegram</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<ScenarioContext?> GetContext(long userId, CancellationToken ct);
    
    /// <summary>
    /// Задать контекст пользователя
    /// </summary>
    /// <param name="userId">Id пользователя в Telegram</param>
    /// <param name="context">Объект класса ScenarioContext</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task? SetContext(long userId, ScenarioContext? context, CancellationToken ct);
    
    /// <summary>
    /// Сбросить (очистить) контекст пользователя
    /// </summary>
    /// <param name="userId">Id пользователя в Telegram</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task ResetContext(long userId, CancellationToken ct);
}
