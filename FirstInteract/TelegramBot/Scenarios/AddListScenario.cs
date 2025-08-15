using FirstInteract.Core.Entities;
using FirstInteract.Core.Services;
using FirstInteract.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FirstInteract.TelegramBot.Scenarios;

public class AddListScenario(IUserService userService, IToDoListService toDoListService) : IScenario
{
    public bool CanHandle(ScenarioType scenario)
    {
        return scenario == ScenarioType.AddList;
    }

    public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext? context,
        Update update, CancellationToken ct)
    {
        Message? message;
        long userId;
        string userName;

        switch (update.Type)
        {
            case UpdateType.CallbackQuery:
            {
                message = update.CallbackQuery!.Message;
                // Пользователь, который нажал на кнопку "Добавить список"
                userId = update.CallbackQuery!.From.Id;
                userName = update.CallbackQuery.From.Username!;
                break;
            }
            case UpdateType.Message:
            {
                message = update.Message;
                userId = message!.From!.Id;
                userName = message.From.Username!;
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (message == null)
            return ScenarioResult.Completed;

        switch (context!.CurrentStep)
        {
            case null:
            {
                var user = await userService.GetUser(userId, ct) ??
                           await userService.RegisterUser(userId, userName, ct);
                context.Data["ToDoUser"] = user;
                await bot.SendMessageWithCancelButton(message.Chat, text: "Введите название списка:",
                    cancellationToken: ct);
                context.CurrentStep = "Name";
                return ScenarioResult.Transition;
            }
            case "Name":
            {
                var listName = message.Text?.Trim();
                await toDoListService.Add((ToDoUser)context.Data["ToDoUser"], listName!, ct);
                await bot.SendMessageWithDefaultButtons(message.Chat, text: $"Список \"{listName}\" успешно добавлен",
                    cancellationToken: ct);
                context.CurrentStep = null; //для корректной работы клавиатуры
                return ScenarioResult.Completed;
            }
            default:
                return ScenarioResult.Completed;
        }
    }
}