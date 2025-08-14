using FirstInteract.Core.Entities;
using FirstInteract.Core.Services;
using FirstInteract.Helpers;
using FirstInteract.TelegramBot.Dto;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FirstInteract.TelegramBot.Scenarios;

public class DeleteTaskScenario(IUserService userService, IToDoService toDoService) : IScenario
{
    public bool CanHandle(ScenarioType scenario)
    {
        return scenario == ScenarioType.DeleteTask;
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
                // Пользователь, который нажал на кнопку "Удалить задачу"
                userId = update.CallbackQuery!.From.Id;
                userName = update.CallbackQuery!.From.Username!;
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

        // Один из вариантов: null, Delete
        switch (context!.CurrentStep)
        {
            case null:
            {
                var user = await userService.GetUser(userId, ct) ??
                           await userService.RegisterUser(userId, userName, ct);
                context.Data["ToDoUser"] = user;
                var toDoItemDto = ToDoItemCallbackDto.FromString(update.CallbackQuery!.Data!);
                var itemGuid = Guid.Parse(toDoItemDto.ToString().Split("|")[1]);
                var toDoItem = await toDoService.Get(itemGuid, ct);
                context.Data["ToDoItem"] = toDoItem!;

                // Создаем список строк для клавиатуры
                var keyboardRows = new List<InlineKeyboardButton[]>();
                keyboardRows.Add(
                    [
                        InlineKeyboardButton.WithCallbackData("✅Да", "yes"),
                        InlineKeyboardButton.WithCallbackData("❌Нет", "no")
                    ]
                );
                // Создаем клавиатуру
                var inlineKeyboard = new InlineKeyboardMarkup(keyboardRows);
                await bot.SendMessage(update.CallbackQuery!.Message!.Chat,
                    text: $"Подтверждаете удаление задачи \"{toDoItem!.Name}\"",
                    replyMarkup: inlineKeyboard, cancellationToken: ct);

                context.CurrentStep = "Delete";
                return ScenarioResult.Transition;
            }

            case "Delete":
            {
                switch (update.CallbackQuery!.Data!)
                {
                    case "yes":
                        var toDoItem = (ToDoItem)context.Data["ToDoItem"];
                        await toDoService.Delete(toDoItem.Id, ct);
                        // Выводим информацию, что задача удалена
                        await bot.SendMessageWithDefaultButtons(update.CallbackQuery.Message!.Chat,
                            $"Задача \"{toDoItem.Name}\" удалена", cancellationToken: ct);
                        break;

                    case "no":
                    {
                        await bot.SendMessageWithDefaultButtons(message.Chat, text: $"Удаление отменено",
                            cancellationToken: ct);
                        break;
                    }
                }

                context.CurrentStep = null;
                return ScenarioResult.Completed;
            }

            default:
                return ScenarioResult.Completed;
        }
    }
}