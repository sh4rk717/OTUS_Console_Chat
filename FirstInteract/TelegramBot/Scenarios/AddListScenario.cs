using FirstInteract.Core.Entities;
using FirstInteract.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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
                var replyMarkup = new ReplyKeyboardMarkup(true).AddNewRow("/cancel");
                await bot.SendMessage(chatId: message.Chat.Id, text: "Введите название списка:", replyMarkup: replyMarkup, cancellationToken: ct);
                context.CurrentStep = "Name";
                return ScenarioResult.Transition;
            }
            case "Name":
            {
                var listName = message.Text?.Trim();
                await toDoListService.Add((ToDoUser)context.Data["ToDoUser"], listName!, ct);
                var replyMarkup = new ReplyKeyboardMarkup(true).AddNewRow("/show").AddNewRow("/addtask", "/report");
                await bot.SendMessage(chatId: message.Chat.Id, text: $"Список \"{listName}\" успешно добавлен", replyMarkup: replyMarkup, cancellationToken: ct);
                context.CurrentStep = null; //для корректной работы клавиатуры
                return ScenarioResult.Completed;
            }
            default:
                return ScenarioResult.Completed;
        }
    }
}