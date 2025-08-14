using FirstInteract.Core.Entities;
using FirstInteract.Core.Services;
using FirstInteract.TelegramBot.Dto;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FirstInteract.TelegramBot.Scenarios;

public class DeleteListScenario(IUserService userService, IToDoListService toDoListService, IToDoService toDoService)
    : IScenario
{
    public bool CanHandle(ScenarioType scenario)
    {
        return scenario == ScenarioType.DeleteList;
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
                // Пользователь, который нажал на кнопку "Удалить список"
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

        // Один из вариантов: null, Approve, Delete
        switch (context!.CurrentStep)
        {
            case null:
            {
                var user = await userService.GetUser(userId, ct) ??
                           await userService.RegisterUser(userId, userName, ct);
                context.Data["ToDoUser"] = user;

                var userLists = await toDoListService.GetUserLists(user.UserId, ct);

                // Создаем список строк для клавиатуры
                var keyboardRows = new List<InlineKeyboardButton[]>();

                // Добавляем все списки пользователя
                if (userLists.Count > 0)
                {
                    keyboardRows.AddRange(userLists.Select(list => (InlineKeyboardButton[])
                    [
                        InlineKeyboardButton.WithCallbackData(list.Name,
                            new ToDoListCallbackDto("deletelist", list.Id).ToString())
                    ]));
                    // Создаем клавиатуру
                    var inlineKeyboard = new InlineKeyboardMarkup(keyboardRows);

                    await bot.SendMessage(chatId: message.Chat, text: "Выберете список для удаления:",
                        replyMarkup: inlineKeyboard, cancellationToken: ct);
                    context.CurrentStep = "Approve";
                    return ScenarioResult.Transition;
                }
                else
                {
                    await bot.SendMessage(chatId: message.Chat, text: "Нет пользовательских списков",
                        cancellationToken: ct);
                    context.CurrentStep = null;
                    return ScenarioResult.Completed;
                }
            }

            case "Approve":
            {
                var toDoListId = Guid.Parse(update.CallbackQuery!.Data!.Split("|")[1]);
                var toDoList = await toDoListService.Get(toDoListId, ct);
                context.Data["ToDoList"] = toDoList!;
                var keyboardRows = new List<InlineKeyboardButton[]>();
                keyboardRows.Add(
                    [
                        InlineKeyboardButton.WithCallbackData("✅Да", "yes"),
                        InlineKeyboardButton.WithCallbackData("❌Нет", "no")
                    ]
                );
                // Создаем клавиатуру
                var inlineKeyboard = new InlineKeyboardMarkup(keyboardRows);
                await bot.SendMessage(chatId: message.Chat,
                    text: $"Подтверждаете удаление списка \"{toDoList!.Name}\" и всех его задач?",
                    replyMarkup: inlineKeyboard, cancellationToken: ct);
                context.CurrentStep = "Delete";
                return ScenarioResult.Transition;
            }

            case "Delete":
            {
                var replyMarkup = new ReplyKeyboardMarkup(true).AddNewRow("/show").AddNewRow("/addtask", "/report");

                //var listName = message.Text?.Trim();
                switch (update.CallbackQuery!.Data!)
                {
                    case "yes":
                        var userGuid = ((ToDoUser)context.Data["ToDoUser"]).UserId;
                        var listGuid = ((ToDoList)context.Data["ToDoList"]).Id;
                        var toDoItemToDelete = await toDoService.GetByUserIdAndList(userGuid, listGuid, ct);
                        // Удаляем все задачи из выбранного списка
                        foreach (var toDoItem in toDoItemToDelete)
                        {
                            await toDoService.Delete(toDoItem.Id, ct);
                        }

                        // Удаляем сам список
                        await toDoListService.Delete(listGuid, ct);
                        await bot.SendMessage(chatId: message.Chat.Id, text: $"Список со всеми задачами удален",
                            replyMarkup: replyMarkup, cancellationToken: ct);
                        break;
                    
                    case "no":
                    {
                        await bot.SendMessage(chatId: message.Chat.Id, text: $"Удаление отменено",
                            replyMarkup: replyMarkup, cancellationToken: ct);
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