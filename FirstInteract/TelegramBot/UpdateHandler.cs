using FirstInteract.Core.Entities;
using FirstInteract.Core.Exceptions;
using FirstInteract.Core.Services;
using FirstInteract.Helpers;
using FirstInteract.TelegramBot.Dto;
using FirstInteract.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FirstInteract.TelegramBot;

public class UpdateHandler(
    ITelegramBotClient botClient,
    IUserService userService,
    IToDoService toDoService,
    IToDoListService toDoListService,
    IToDoReportService toDoReportService,
    IEnumerable<IScenario> scenarios,
    IScenarioContextRepository contextRepository)
    : IUpdateHandler
{
    public delegate void MessageEventHandler(string message);

    public event MessageEventHandler? OnHandleUpdateStarted;
    public event MessageEventHandler? OnHandleUpdateCompleted;

    // Количество кнопок на одной странице Inline-клавиатуры с задачами
    private const int PageSize = 5;

    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        try
        {
            // Switch по типу update в чате
            switch (update)
            {
                case { Message: { } message }:

                    if (message.Text == "/cancel")
                    {
                        await contextRepository.ResetContext(message.From!.Id, ct);
                        await SendReplyKeyboardCommon(update, ct);
                    }

                    var scenarioContext = await contextRepository.GetContext(message.From!.Id, ct);
                    if (scenarioContext is not null)
                    {
                        await ProcessScenario(scenarioContext, update, ct);
                    }

                    var command = message.Text!.Split(" ")[0]; //до 1-ого пробела
                    OnHandleUpdateStarted?.Invoke(message.Text!);
                    var restArgs = string.Join(" ", message.Text.Split(" ")[1..]); //все остальное после 1-ого пробела
                    switch (command)
                    {
                        case "/start":
                            await RunStart(update, ct);
                            await SendReplyKeyboardCommon(update, ct);
                            break;
                        case "/help":
                            await RunHelp(update, ct);
                            await SendReplyKeyboardCommon(update, ct);
                            break;
                        case "/info":
                            await RunInfo(update, ct);
                            await SendReplyKeyboardCommon(update, ct);
                            break;
                        case "/addtask":
                            await AddTask(update, ct);
                            break;
                        case "/show":
                            await Show(update, ct);
                            break;
                        case "/report":
                            await Report(update, toDoReportService, ct);
                            break;
                        case "/find":
                            await Find(update, restArgs, ct);
                            break;
                    }

                    // асинхронно выводим кнопку menu с командами
                    await ShowNativeCommands(ct);
                    OnHandleUpdateCompleted?.Invoke(message.Text);
                    break;

                case { CallbackQuery: { } callbackQuery }:
                    await OnCallbackQuery(update, callbackQuery, ct);
                    break;
            }
        }
        catch (ArgumentException e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
            Console.ResetColor();
            throw;
        }
        catch (TaskCountLimitException e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
            Console.ResetColor();
            throw;
        }
        catch (TaskLengthLimitException e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
            Console.ResetColor();
            throw;
        }
        catch (DuplicateTaskException e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
            Console.ResetColor();
            throw;
        }
    }

    private async Task OnCallbackQuery(Update update, CallbackQuery callbackQuery, CancellationToken ct)
    {
        // Проверка регистрации пользователя
        var user = await userService.GetUser(callbackQuery.From.Id, ct);
        if (user == null)
        {
            await botClient.AnswerCallbackQuery(callbackQuery.Id, "Пользователь не зарегистрирован",
                cancellationToken: ct);
            return;
        }

        // Проверка активного сценария
        var scenarioContextCallback = await contextRepository.GetContext(callbackQuery.From.Id, ct);
        if (scenarioContextCallback is not null)
        {
            await ProcessScenario(scenarioContextCallback, update, ct);
            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
            return;
        }

        // Обработка CallbackDto
        var callbackDto = CallbackDto.FromString(callbackQuery.Data!);

        // Switch по действиям в CallbackQuery
        switch (callbackDto.Action)
        {
            // Обработка кнопок "Без списка" или выбранный список
            case "show":
            {
                var listDto = PagedListCallbackDto.FromString(callbackQuery.Data!);
                var tasks = await toDoService.GetByUserIdAndList(user.UserId, listDto.ToDoListId, ct);
                // Все задачи пользователя в конкретном списке (активные, завершенные)
                var allTasksInList = (await toDoService.GetAllByUserId(user.UserId, ct))
                    .Where(item => item.List?.Id == listDto.ToDoListId /* && item.State == specificState*/)
                    .ToList();

                if (allTasksInList.Count == 0)
                {
                    await botClient.SendMessage(callbackQuery.Message!.Chat, "В этом списке нет задач",
                        cancellationToken: ct);
                }
                else
                {
                    var userListTasks = await toDoService.GetByUserIdAndList(user.UserId, listDto.ToDoListId, ct);
                    IReadOnlyList<KeyValuePair<string, string>> a = Array.AsReadOnly(
                        userListTasks
                            .Select(item => new KeyValuePair<string, string>(item.Name, item.Id.ToString()))
                            .ToArray());

                    // Создаем клавиатуру
                    var inlineKeyboard = BuildPagedButtons(a,
                        new PagedListCallbackDto("show", listDto.ToDoListId, listDto.Page) /*, true*/);

                    // Преобразуем в список для редактирования
                    var keyboardRows = inlineKeyboard.InlineKeyboard.ToList();

                    // Добавляем новую строку с кнопкой внизу
                    keyboardRows.Add([
                        InlineKeyboardButton.WithCallbackData("☑️ Посмотреть выполненные",
                            new PagedListCallbackDto("show_completed", listDto.ToDoListId, 0).ToString())
                    ]);

                    // Создаем обновленную клавиатуру
                    inlineKeyboard = new InlineKeyboardMarkup(keyboardRows);

                    await botClient.EditMessageText(
                        chatId: update.CallbackQuery!.Message!.Chat,
                        messageId: callbackQuery.Message!.Id,
                        text: tasks.Count > 0 ? "Активные задачи:" : "Активных задач в этом списке нет",
                        replyMarkup: inlineKeyboard,
                        cancellationToken: ct);
                }

                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                break;
            }

            // Обработка кнопки "Посмотреть выполненные" после выбора списка
            case "show_completed":
                var listDto2 = PagedListCallbackDto.FromString(callbackQuery.Data!);
                var completedTasks = (await toDoService.GetAllByUserId(user.UserId, ct))
                    .Where(item => item.List?.Id == listDto2.ToDoListId
                                   && item.State == ToDoItem.ToDoItemState.Completed)
                    .ToList();

                if (completedTasks.Count == 0)
                {
                    await botClient.SendMessage(callbackQuery.Message!.Chat, "В этом списке нет выполненных задач",
                        cancellationToken: ct);
                }
                else
                {
                    IReadOnlyList<KeyValuePair<string, string>> a = Array.AsReadOnly(
                        completedTasks
                            .Select(item => new KeyValuePair<string, string>(item.Name, item.Id.ToString()))
                            .ToArray());

                    var pagedListCallbackDto = new PagedListCallbackDto("show_completed", listDto2.ToDoListId,
                        listDto2.Page);
                    // Создаем клавиатуру
                    var inlineKeyboard = BuildPagedButtons(a, pagedListCallbackDto);
                    await botClient.EditMessageText(
                        chatId: update.CallbackQuery!.Message!.Chat,
                        messageId: callbackQuery.Message!.Id,
                        text: "Выполненные задачи:",
                        replyMarkup: inlineKeyboard,
                        cancellationToken: ct);
                }

                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                break;

            // Обработка кнопки "🆕 Добавить" (addlist)
            case "addlist":
                scenarioContextCallback = new ScenarioContext(callbackQuery.From.Id, ScenarioType.AddList);
                await ProcessScenario(scenarioContextCallback, update, ct);
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                break;

            // Обработка кнопки "❌ Удалить" (deletelist)
            case "deletelist":
                //Если NULL, то присваиваем значение
                scenarioContextCallback ??= new ScenarioContext(callbackQuery.From.Id, ScenarioType.DeleteList);
                await ProcessScenario(scenarioContextCallback, update, ct);
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                break;

            // Обработка кнопки выбора списка для задачи (addtask)
            case "addtask":
                //Если NULL, то присваиваем значение
                scenarioContextCallback ??= new ScenarioContext(callbackQuery.From.Id, ScenarioType.AddTask);
                await ProcessScenario(scenarioContextCallback, update, ct);
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                break;

            // Выводим информацию по задаче
            case "showtask":
                var toDoItemDto = ToDoItemCallbackDto.FromString(callbackQuery.Data!);
                var itemGuid = Guid.Parse(toDoItemDto.ToString().Split("|")[1]);
                var toDoItem = await toDoService.Get(itemGuid, ct);

                switch (toDoItem!.State)
                {
                    case ToDoItem.ToDoItemState.Active:
                    {
                        var activeTaskInfo =
                            $"{toDoItem.Name}\n" +
                            $"\n" +
                            $"Срок выполнения: {toDoItem.Deadline}\n" +
                            $"Время создания: {toDoItem.CreatedAt}";

                        // Создаем список строк для клавиатуры
                        var keyboardRows = new List<InlineKeyboardButton[]>();

                        // Добавляем кнопки действий
                        keyboardRows.Add([
                            InlineKeyboardButton.WithCallbackData("✅Выполнить",
                                new ToDoItemCallbackDto("completetask", itemGuid).ToString()),
                            InlineKeyboardButton.WithCallbackData("❌Удалить",
                                new ToDoItemCallbackDto("deletetask", itemGuid).ToString())
                        ]);

                        // Создаем клавиатуру
                        var inlineKeyboard = new InlineKeyboardMarkup(keyboardRows);

                        // Выводим информацию по задаче с кнопками
                        await botClient.SendMessage(
                            chatId: update.CallbackQuery!.Message!.Chat,
                            text: activeTaskInfo,
                            replyMarkup: inlineKeyboard,
                            cancellationToken: ct);
                        break;
                    }
                    case ToDoItem.ToDoItemState.Completed:
                    {
                        var completedTaskInfo =
                            $"{toDoItem.Name}\n" +
                            $"\n" +
                            $"Срок выполнения: {toDoItem.Deadline}\n" +
                            $"Время создания: {toDoItem.CreatedAt}\n" +
                            $"Выполнена: {toDoItem.StateChangedAt}";

                        // Выводим информацию по задаче с кнопками
                        await botClient.SendMessage(
                            chatId: update.CallbackQuery!.Message!.Chat,
                            text: completedTaskInfo,
                            cancellationToken: ct);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                break;

            case "completetask":
                toDoItemDto = ToDoItemCallbackDto.FromString(callbackQuery.Data!);
                itemGuid = Guid.Parse(toDoItemDto.ToString().Split("|")[1]);
                toDoItem = await toDoService.Get(itemGuid, ct);
                await toDoService.MarkCompleted(itemGuid, ct);
                // Выводим информацию, что задача выполнена
                await botClient.SendMessageWithDefaultButtons(update.CallbackQuery!.Message!.Chat,
                    $"Задача \"{toDoItem!.Name}\" выполнена", cancellationToken: ct);

                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                break;

            case "deletetask":
                //Если NULL, то присваиваем значение
                scenarioContextCallback ??= new ScenarioContext(callbackQuery.From.Id, ScenarioType.DeleteTask);
                await ProcessScenario(scenarioContextCallback, update, ct);
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                break;
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"HandleError: {exception})");
        return Task.CompletedTask;
    }

    private async Task RunStart(Update update, CancellationToken ct)
    {
        await userService.RegisterUser(update.Message!.From!.Id, update.Message.From.Username!, ct);
        await botClient.SendMessage(update.Message.Chat, "Пользователь зарегистрирован", cancellationToken: ct);
    }

    private async Task RunHelp(Update update, CancellationToken ct)
    {
        await botClient.SendMessage(update.Message!.Chat,
            """
            "To Do" Telegram-бот
            Пользователю доступен набор команд...
            /start - регистрация пользователя
            /help - помощь
            /info - информация о программе
            Команды доступные после регистрации пользователя:
            /addtask - позволяет добавить задачу в список задач
            /show - позволяет просмотреть списки задач
            /report - отчет по задачам пользователя
            /find - поиск задач по началу их названия
            /cancel - отмена текущего сценария
            """, cancellationToken: ct);
    }

    private async Task RunInfo(Update update, CancellationToken ct)
    {
        await botClient.SendMessage(update.Message!.Chat, """
                                                          Program info: version 1.0f.
                                                          Created: Feb 18, 2025
                                                          Last updated: August 14, 2025
                                                          """, cancellationToken: ct);
    }

    private async Task AddTask(Update update, CancellationToken ct)
    {
        var user = await userService.GetUser(update.Message!.From!.Id, ct);

        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (user == null)
        {
            await botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован",
                cancellationToken: ct);
            await SendReplyKeyboardStart(update, ct);
            return;
        }

        var scenarioContext = new ScenarioContext(update.Message!.From!.Id, ScenarioType.AddTask);
        await ProcessScenario(scenarioContext, update, ct);
    }

    private async Task Show(Update update, CancellationToken ct)
    {
        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (await userService.GetUser(update.Message!.From!.Id, ct) == null)
        {
            await botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован",
                cancellationToken: ct);
            await SendReplyKeyboardStart(update, ct);
            return;
        }

        await ListInlineKeyboard(update, ct);
    }

    private async Task ListInlineKeyboard(Update update, CancellationToken ct)
    {
        var user = await userService.GetUser(update.Message!.From!.Id, ct);
        var userLists = await toDoListService.GetUserLists(user!.UserId, ct);

        // Создаем список строк для клавиатуры
        var keyboardRows = new List<InlineKeyboardButton[]>();

        // Добавляем кнопку "Без списка"
        keyboardRows.Add([
            InlineKeyboardButton.WithCallbackData("📌 Без списка",
                new PagedListCallbackDto("show", null, page: 0).ToString()
            )
        ]);

        // Добавляем все списки пользователя
        if (userLists.Count > 0)
            keyboardRows.AddRange(userLists.Select(list => (InlineKeyboardButton[])
            [
                InlineKeyboardButton.WithCallbackData(list.Name,
                    new PagedListCallbackDto("show", list.Id, page: 0).ToString())
            ]));

        // Добавляем кнопки действий в последнюю строку
        keyboardRows.Add([
            InlineKeyboardButton.WithCallbackData("🆕 Добавить", "addlist"),
            InlineKeyboardButton.WithCallbackData("❌ Удалить", "deletelist")
        ]);

        // Создаем клавиатуру
        var inlineKeyboard = new InlineKeyboardMarkup(keyboardRows);

        var inlineMessage = await botClient.SendMessage(
            chatId: update.Message!.Chat,
            text: "Выберите список:",
            replyMarkup: inlineKeyboard,
            cancellationToken: ct);

        Console.WriteLine(inlineMessage.Id);
    }

    private async Task Report(Update update, IToDoReportService toDoReport,
        CancellationToken ct)
    {
        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (await userService.GetUser(update.Message!.From!.Id, ct) == null)
        {
            await botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован",
                cancellationToken: ct);
            await SendReplyKeyboardStart(update, ct);
            return;
        }

        var user = await userService.GetUser(update.Message.From.Id, ct);
        var (total, completed, active, generatedAt) = await toDoReport.GetUserStats(user!.UserId, ct);
        await botClient.SendMessage(update.Message.Chat,
            $"Статистика по задачам на {generatedAt}. Всего: {total}; Завершенных: {completed}; Активных: {active};",
            cancellationToken: ct);
    }

    private async Task Find(Update update, string taskStartsWithString,
        CancellationToken ct)
    {
        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (await userService.GetUser(update.Message!.From!.Id, ct) == null)
        {
            await botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован",
                cancellationToken: ct);
            await SendReplyKeyboardStart(update, ct);
            return;
        }

        taskStartsWithString = Program.ValidateString(taskStartsWithString);

        var user = await userService.GetUser(update.Message.From.Id, ct);
        var itemList = await toDoService.Find(user!, taskStartsWithString, ct);

        await botClient.SendMessage(update.Message.Chat, "Список найденных задач:", cancellationToken: ct);
        if (itemList.Count == 0) await botClient.SendMessage(update.Message.Chat, "пуст", cancellationToken: ct);
        else
        {
            var index = 1;
            foreach (var item in itemList)
            {
                await botClient.SendMessage(update.Message.Chat,
                    $"({item.State}) {index++}. {item.Name} - {item.CreatedAt} - {item.Id}", cancellationToken: ct);
            }
        }
    }

    private async Task SendReplyKeyboardStart(Update update, CancellationToken ct = default)
    {
        //если пользователь не зарегистрирован, то просим зарегистрироваться
        if (await userService.GetUser(update.Message!.From!.Id, ct) == null)
        {
            var replyMarkup = new ReplyKeyboardMarkup(true).AddNewRow("/start");
            await botClient.SendMessage(update.Message.Chat, "Please, register", replyMarkup: replyMarkup,
                cancellationToken: ct);
        }
    }

    private async Task SendReplyKeyboardCommon(Update update, CancellationToken ct = default)
    {
        //если пользователь не зарегистрирован, то просим зарегистрироваться
        if (await userService.GetUser(update.Message!.From!.Id, ct) == null)
        {
            var replyMarkup = new ReplyKeyboardMarkup(true).AddNewRow("/start");
            await botClient.SendMessage(update.Message.Chat, "Please, register", replyMarkup: replyMarkup,
                cancellationToken: ct);
        }
        else
        {
            await botClient.SendMessageWithDefaultButtons(update.Message.Chat, "Выберите команду:",
                cancellationToken: ct);
        }
    }

    private async Task ShowNativeCommands(CancellationToken ct = default)
    {
        var commands = new List<BotCommand>
        {
            new() { Command = "start", Description = "Запустить бота" },
            new() { Command = "help", Description = "Помощь" },
            new() { Command = "info", Description = "Информация о боте" },
            new() { Command = "addtask", Description = "Добавить задачу" },
            new() { Command = "show", Description = "Показать активные задачи" },
            new() { Command = "report", Description = "Отчет по задачам" },
            new() { Command = "cancel", Description = "Отменить текущий сценарий" }
        };

        await botClient.SetMyCommands(commands: commands, cancellationToken: ct);
    }

    private IScenario GetScenario(ScenarioType scenarioType)
    {
        foreach (var scenario in scenarios)
        {
            if (scenario.CanHandle(scenarioType))
            {
                return scenario;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(scenarioType));
    }

    private async Task ProcessScenario(ScenarioContext? context, Update update, CancellationToken ct)
    {
        var scenario = GetScenario(context!.CurrentScenario);
        var userId = context.UserId;
        var result = await scenario.HandleMessageAsync(botClient, context, update, ct);
        if (result == ScenarioResult.Completed)
            await contextRepository.ResetContext(userId, ct);
        else
            await contextRepository.SetContext(userId, context, ct)!;
    }

    private InlineKeyboardMarkup BuildPagedButtons(IReadOnlyList<KeyValuePair<string, string>> callbackData,
        PagedListCallbackDto listDto)
    {
        var totalPages = (callbackData.Count - 1) / PageSize + 1;

        // Создаем список строк для клавиатуры
        var keyboardRows = new List<InlineKeyboardButton[]>();
        // Создаем список для кнопок пагинации
        var paginationButtons = new List<InlineKeyboardButton>();

        var userListTasks = callbackData.GetBatchByNumber(PageSize, listDto.Page);

        keyboardRows.AddRange(userListTasks
            .Select(toDoItem => (InlineKeyboardButton[])
            [
                InlineKeyboardButton.WithCallbackData(toDoItem.Key,
                    // new PagedListCallbackDto(listDto.Action, listDto.ToDoListId, page: listDto.Page).ToString())
                    new ToDoItemCallbackDto("showtask", Guid.Parse(toDoItem.Value)).ToString())
            ]));

        if (listDto.Page > 0)
            paginationButtons.Add(
                InlineKeyboardButton.WithCallbackData("⬅️",
                    new PagedListCallbackDto(listDto.Action, listDto.ToDoListId, listDto.Page - 1).ToString())
            );
        if (listDto.Page < totalPages - 1)
            paginationButtons.Add(
                InlineKeyboardButton.WithCallbackData("➡️",
                    new PagedListCallbackDto(listDto.Action, listDto.ToDoListId, listDto.Page + 1).ToString())
            );

        // Если есть хотя бы одна кнопка пагинации, добавляем их в одну строку
        if (paginationButtons.Count > 0)
        {
            keyboardRows.Add(paginationButtons.ToArray());
        }

        // Создаем клавиатуру
        return new InlineKeyboardMarkup(keyboardRows);
    }
}