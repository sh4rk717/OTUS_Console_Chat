namespace FirstInteract.TelegramBot.Scenarios;

/// <summary>
/// Класс, который хранит информацию о контексте (сессии) пользователя
/// </summary>
public class ScenarioContext(long userId, ScenarioType scenario)
{
    /// <summary>
    /// Id пользователя в Telegram
    /// </summary>
    public long UserId { get; set; } = userId;

    public ScenarioType CurrentScenario { get; } = scenario;

    /// <summary>
    /// Текущий шаг сценария
    /// </summary>
    public string? CurrentStep { get; set; }

    /// <summary>
    /// Дополнительная информация, необходимая для работы сценария 
    /// </summary>
    public Dictionary<string, object> Data { get; } = new();
}

/// <summary>
/// Хранятся все поддерживаемые сценарии
/// </summary>
public enum ScenarioType
{
    None,
    AddTask,
    AddList,
    DeleteList,
    DeleteTask
}

/// <summary>
/// Нужен для получения результата выполнения сценария
/// Transition - Переход к следующему шагу. Сообщение обработано, но сценарий еще не завершен
/// Completed - Сценарий завершен
/// </summary>
public enum ScenarioResult
{
    Transition,
    Completed
}