using System.Collections.Concurrent;

namespace FirstInteract.TelegramBot.Scenarios;

public class InMemoryScenarioContextRepository : IScenarioContextRepository
{
    /// <summary>
    /// Хранилище
    /// </summary>
    private readonly ConcurrentDictionary<long, ScenarioContext?> _scenarioContexts = new();

    public Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
    {
        return (!_scenarioContexts.TryGetValue(userId, out var value)
            ? Task.FromResult<ScenarioContext?>(null)
            : Task.FromResult(value));
    }

    public Task SetContext(long userId, ScenarioContext? context, CancellationToken ct)
    {
        _scenarioContexts[userId] = context;
        return Task.CompletedTask;
    }

    public Task ResetContext(long userId, CancellationToken ct)
    {
        _scenarioContexts.TryRemove(userId, out _);
        return Task.CompletedTask;
    }
}