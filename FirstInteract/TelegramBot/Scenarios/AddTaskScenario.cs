using System.Globalization;
using FirstInteract.Core.Entities;
using FirstInteract.Core.Services;
using FirstInteract.TelegramBot.Dto;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using FirstInteract.Helpers;

namespace FirstInteract.TelegramBot.Scenarios;

public class AddTaskScenario(IUserService userService, IToDoService toDoService, IToDoListService toDoListService)
    : IScenario
{
    public bool CanHandle(ScenarioType scenario)
    {
        return scenario == ScenarioType.AddTask;
    }

    public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext? context,
        Update update, CancellationToken ct)
    {
        var message = update.Type switch
        {
            UpdateType.CallbackQuery => update.CallbackQuery!.Message,
            UpdateType.Message => update.Message,
            _ => throw new ArgumentOutOfRangeException(nameof(update))
        };

        if (message == null)
            return ScenarioResult.Completed;

        switch (context!.CurrentStep)
        {
            case null:
            {
                var user = await userService.GetUser(message.From!.Id, ct) ??
                           await userService.RegisterUser(message.From.Id, message.From.Username ?? string.Empty, ct);
                context.Data["ToDoUser"] = user;
                await bot.SendMessageWithCancelButton(message.Chat, text: "Введите название задачи:",
                    cancellationToken: ct);
                context.CurrentStep = "Name";
                return ScenarioResult.Transition;
            }
            case "Name":
            {
                var taskName = message.Text?.Trim();
                await bot.SendMessage(chatId: message.Chat.Id,
                    text: $"Задайте deadline для задачи '{taskName}' в формате dd.MM.yyyy:", cancellationToken: ct);
                context.CurrentStep = "Deadline";
                context.Data["TaskName"] = taskName!;
                return ScenarioResult.Transition;
            }
            case "Deadline":
            {
                var user = (ToDoUser)context.Data["ToDoUser"];
                var isDate = DateTime.TryParseExact(message.Text!.Trim(),
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var taskDeadline);

                if (!isDate)
                {
                    await bot.SendMessage(chatId: message.Chat.Id,
                        text: $"Задана невалидная дата! Введите корректную дату:", cancellationToken: ct);
                    return ScenarioResult.Transition; //идем на повторный запрос даты
                }

                // Готовим и выводим Inline-клавиатуру
                var userLists = await toDoListService.GetUserLists(user.UserId, ct);
                // Создаем список строк для клавиатуры
                var keyboardRows = new List<InlineKeyboardButton[]>();
                keyboardRows.Add([
                    InlineKeyboardButton.WithCallbackData("📌 Без списка",
                        new ToDoListCallbackDto("addtask", null).ToString())
                ]);
                // Добавляем все списки пользователя
                if (userLists.Count > 0)
                {
                    keyboardRows.AddRange(userLists.Select(list => (InlineKeyboardButton[])
                    [
                        InlineKeyboardButton.WithCallbackData(list.Name,
                            new ToDoListCallbackDto("addtask", list.Id).ToString())
                    ]));
                }

                // Создаем клавиатуру
                var inlineKeyboard = new InlineKeyboardMarkup(keyboardRows);

                await bot.SendMessage(chatId: message.Chat, text: "Выберете список для задачи:",
                    replyMarkup: inlineKeyboard, cancellationToken: ct);

                context.Data["Deadline"] = taskDeadline;
                context.CurrentStep = "ChooseList";
                return ScenarioResult.Transition;
            }
            case "ChooseList":
            {
                var user = (ToDoUser)context.Data["ToDoUser"];
                var taskName = (string)context.Data["TaskName"];
                var deadline = (DateTime)context.Data["Deadline"];
                var isGuid = Guid.TryParse(update.CallbackQuery!.Data!.Split("|")[1], out var toDoListId);
                if (isGuid)
                {
                    var toDoList = await toDoListService.Get(toDoListId, ct);
                    await toDoService.Add(user, taskName, deadline, toDoList, ct);
                    await bot.SendMessageWithDefaultButtons(message.Chat,
                        text: $"Задача '{taskName}' добавлена в список {toDoList!.Name}! Крайний срок: {deadline}",
                        cancellationToken: ct);
                }
                else
                {
                    await toDoService.Add(user, taskName, deadline, null, ct);
                    await bot.SendMessageWithDefaultButtons(message.Chat,
                        text: $"Задача '{taskName}' добавлена (вне списка)! Крайний срок: {deadline}",
                        cancellationToken: ct);
                }

                context.CurrentStep = null; //для корректной работы клавиатуры
                return ScenarioResult.Completed;
            }

            default:
                return ScenarioResult.Completed;
        }
    }
}