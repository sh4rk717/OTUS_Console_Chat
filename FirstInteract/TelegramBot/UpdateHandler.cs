using FirstInteract.Core.Exceptions;
using FirstInteract.Core.Services;
using FirstInteract.TelegramBot.Dto;
using FirstInteract.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
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
                        case "/removetask":
                            await RemoveTask(update, restArgs, ct);
                            break;
                        case "/completetask":
                            await CompleteTask(update, restArgs, ct);
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
                            var listDto = ToDoListCallbackDto.FromString(callbackQuery.Data!);
                            var tasks = await toDoService.GetByUserIdAndList(user.UserId, listDto.ToDoListId, ct);

                            if (tasks.Count == 0)
                            {
                                await botClient.SendMessage(callbackQuery.Message!.Chat, "В этом списке нет задач",
                                    cancellationToken: ct);
                            }
                            else
                            {
                                await botClient.SendMessage(callbackQuery.Message!.Chat, $"Список активных задач:",
                                    cancellationToken: ct);
                                var index = 1;
                                foreach (var item in tasks)
                                {
                                    await botClient.SendMessage(callbackQuery.Message.Chat,
                                        $"{index++}. {item.Name} - {item.CreatedAt}\n<code>{item.Id}</code>",
                                        cancellationToken: ct, parseMode: ParseMode.Html);
                                }
                            }

                            var replyMarkup = new ReplyKeyboardMarkup(true)
                                .AddNewRow("/show")
                                .AddNewRow("/addtask", "/report");
                            await botClient.SendMessage(update.CallbackQuery.Message!.Chat, "Выберите команду:",
                                replyMarkup: replyMarkup,
                                cancellationToken: ct);

                            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                            return;
                        }

                        // Обработка кнопки "🆕 Добавить" (addlist)
                        case "addlist":
                            scenarioContextCallback = new ScenarioContext(callbackQuery.From.Id, ScenarioType.AddList);
                            await ProcessScenario(scenarioContextCallback, update, ct);
                            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                            break;

                        // Обработка кнопки "❌ Удалить" (deletelist)
                        case "deletelist":
                            //Если NULL, то присваиваем значение
                            scenarioContextCallback ??=
                                new ScenarioContext(callbackQuery.From.Id, ScenarioType.DeleteList);
                            await ProcessScenario(scenarioContextCallback, update, ct);
                            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                            break;

                        // Обработка кнопки выбора списка для задачи (addtask)
                        case "addtask":
                            //Если NULL, то присваиваем значение
                            scenarioContextCallback ??=
                                new ScenarioContext(callbackQuery.From.Id, ScenarioType.AddTask);
                            await ProcessScenario(scenarioContextCallback, update, ct);
                            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                            break;
                    }

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
            /removetask - позволяет удалить задачу из списка задач по ее порядковому номеру
            /completetask - позволяет пометить задачу как завершенную по ее GUID
            /report - отчет по задачам пользователя
            /find - поиск задач по началу их названия
            """, cancellationToken: ct);
    }

    private async Task RunInfo(Update update, CancellationToken ct)
    {
        await botClient.SendMessage(update.Message!.Chat, """
                                                          Program info: version 1.0e.
                                                          Created: Feb 18, 2025
                                                          Last updated: August 09, 2025
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
            InlineKeyboardButton.WithCallbackData(
                "📌 Без списка",
                new ToDoListCallbackDto("show", null).ToString())
        ]);

        // Добавляем все списки пользователя
        if (userLists.Count > 0)
            keyboardRows.AddRange(userLists.Select(list => (InlineKeyboardButton[])
            [
                InlineKeyboardButton.WithCallbackData(list.Name,
                    new ToDoListCallbackDto("show", list.Id).ToString())
            ]));

        // Добавляем кнопки действий в последнюю строку
        keyboardRows.Add([
            InlineKeyboardButton.WithCallbackData("🆕 Добавить", "addlist"),
            InlineKeyboardButton.WithCallbackData("❌ Удалить", "deletelist")
        ]);

        // Создаем клавиатуру
        var inlineKeyboard = new InlineKeyboardMarkup(keyboardRows);

        await botClient.SendMessage(
            chatId: update.Message!.Chat,
            text: "Выберите список:",
            replyMarkup: inlineKeyboard,
            cancellationToken: ct);
    }

    private async Task RemoveTask(Update update, string userInputTaskToRemove, CancellationToken ct)
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
        var taskList = await toDoService.GetAllByUserId(user!.UserId, ct);
        var taskCount = taskList.Count;

        if (taskCount == 0)
        {
            await botClient.SendMessage(update.Message.Chat, "Ваш список задач пуст\nНет задач для удаления",
                cancellationToken: ct);

            return;
        }

        var numberToRemove = 0;
        var isInt = false;
        while (numberToRemove <= 0 || numberToRemove > taskCount || !isInt)
        {
            userInputTaskToRemove = Program.ValidateString(userInputTaskToRemove);
            if (userInputTaskToRemove == "q") return;

            isInt = int.TryParse(userInputTaskToRemove, out numberToRemove);

            if (numberToRemove > 0 && numberToRemove <= taskCount && isInt) continue;

            await botClient.SendMessage(update.Message.Chat, "Некорректный номер задачи для удаления",
                cancellationToken: ct);
            await botClient.SendMessage(update.Message.Chat,
                "Введите номер задачи или нажмите q для выхода из режима удаления: ", cancellationToken: ct);
        }

        var taskToRemove = taskList[numberToRemove - 1];
        await toDoService.Delete(taskToRemove.Id, ct);
        await botClient.SendMessage(update.Message.Chat, $"Задача \"{taskToRemove.Name}\" удалена.",
            cancellationToken: ct);
    }

    private async Task CompleteTask(Update update, string userInputTaskToMarkComplete,
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
        var itemList = await toDoService.GetActiveByUserId(user!.UserId, ct);

        if (itemList.Count == 0)
        {
            await botClient.SendMessage(update.Message.Chat,
                "Ваш список задач пуст\nНет задач чтоб пометить их как выполненные", cancellationToken: ct);
            return;
        }

        foreach (var item in itemList.Where(x => x.Id == Guid.Parse(userInputTaskToMarkComplete)))
        {
            await toDoService.MarkCompleted(item.Id, ct);
            await botClient.SendMessage(update.Message.Chat, $"Задача \"{item.Name}\" помечена исполненной.",
                cancellationToken: ct);
        }
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
            var replyMarkup = new ReplyKeyboardMarkup(true)
                .AddNewRow("/show")
                .AddNewRow("/addtask", "/report");
            await botClient.SendMessage(update.Message.Chat, "Выберите команду:", replyMarkup: replyMarkup,
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
            new() { Command = "report", Description = "Отчет по задачам" }
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
}