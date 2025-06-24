using FirstInteract.Core.Entities;
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

    public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext? context,
        Update update, CancellationToken ct)
    {
        var message = update.Message;
        if (message == null)
            return ScenarioResult.Completed;

        switch (context!.CurrentStep)
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
                //var user = context.Data["ToDoUser"];
                var taskName = message.Text?.Trim();
                //await toDoService.Add(user, taskName!, ct);
                await bot.SendMessage(chatId: message.Chat.Id,
                    text: $"Задайте deadline для задачи '{taskName}' в формате dd.MM.yyyy:", cancellationToken: ct);
                context.CurrentStep = "Deadline";
                context.Data["TaskName"] = taskName!;
                return ScenarioResult.Transition;
            }
            case "Deadline":
            {
                var user = (ToDoUser)context.Data["ToDoUser"];
                var taskName = (string)context.Data["TaskName"];
                var isDate = DateTime.TryParse(message.Text!.Trim(), out var taskDeadline);

                //var taskDeadline = DateTime.TryParse()Parse(message.Text!.Trim());

                if (!isDate)
                {
                    await bot.SendMessage(chatId: message.Chat.Id,
                        text: $"Задана невалидная дата! Введите корректную дату:", cancellationToken: ct);
                    return ScenarioResult.Transition; //идем на повторный запрос даты
                }

                await toDoService.Add(user, taskName, taskDeadline, ct);
                await bot.SendMessage(chatId: message.Chat.Id,
                    text: $"Задача '{taskName}' добавлена! Крайний срок: {taskDeadline}",
                    cancellationToken: ct);
                context.CurrentStep = null; //для корректной работы клавиатуры
                return ScenarioResult.Completed;
            }

            default:
                return ScenarioResult.Completed;
        }
    }
}