using FirstInteract.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FirstInteract.TelegramBot.Scenarios;

public class AddTaskScenario(IUserService userService, IToDoService toDoService) : IScenario
{
    // private readonly IUserService _userService = userService;
    // private readonly IToDoService _toDoService = toDoService;

    public bool CanHandle(ScenarioType scenario)
    {
        return scenario == ScenarioType.AddTask;
    }

    public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext? context, Update update, CancellationToken ct)
    {
        var message = update.Message;
        if (message == null)
            return ScenarioResult.Completed;

        switch (context.CurrentStep)
        {
            case null:
            {
                var user = await userService.GetUser(message.From!.Id, ct) ??
                           await userService.RegisterUser(message.From.Id, message.From.Username ?? string.Empty, ct);
                context.Data["ToDoUser"] = user;
                await bot.SendMessage(chatId: message.Chat.Id, text: "Введите название задачи:", cancellationToken: ct);
                context.CurrentStep = "Name";
                return ScenarioResult.Transition;
            }
            case "Name":
            {
                if (!context.Data.TryGetValue("ToDoUser", out var userObj) || userObj is not Core.Entities.ToDoUser user)
                {
                    await bot.SendMessage(chatId: message.Chat.Id, text: "Ошибка: пользователь не найден в контексте.", cancellationToken: ct);
                    return ScenarioResult.Completed;
                }
                var taskName = message.Text?.Trim();
                await toDoService.Add(user, taskName!, ct);
                await bot.SendMessage(chatId: message.Chat.Id, text: $"Задача '{taskName}' добавлена!", cancellationToken: ct);
                return ScenarioResult.Completed;
            }
            default:
                return ScenarioResult.Completed;
        }
    }
}
