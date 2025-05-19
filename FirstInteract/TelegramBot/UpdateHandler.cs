using FirstInteract.Core.Entities;
using FirstInteract.Core.Exceptions;
using FirstInteract.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FirstInteract.TelegramBot;

public class UpdateHandler(IUserService userService, IToDoService toDoService, IToDoReportService toDoReportService)
    : IUpdateHandler
{
    public delegate void MessageEventHandler(string message);

    public event MessageEventHandler? OnHandleUpdateStarted;
    public event MessageEventHandler? OnHandleUpdateCompleted;

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        try
        {
            OnHandleUpdateStarted?.Invoke(update.Message.Text);

            var command = update.Message.Text.Split(" ")[0]; //до 1-ого пробела
            var restArgs = string.Join(" ", update.Message.Text.Split(" ")[1..]); //все остальное после 1-ого пробела
            switch (command)
            {
                case "/start":
                    await RunStart(botClient, update, ct);
                    break;
                case "/help":
                    await RunHelp(botClient, update, ct);
                    break;
                case "/info":
                    await RunInfo(botClient, update, ct);
                    break;
                case "/addtask":
                    await AddTask(botClient, update, restArgs, ct);
                    break;
                case "/showtasks":
                    await ShowTasks(botClient, update, ct);
                    break;
                case "/removetask":
                    await RemoveTask(botClient, update, restArgs, ct);
                    break;
                case "/completetask":
                    await CompleteTask(botClient, update, restArgs, ct);
                    break;
                case "/showalltasks":
                    await ShowAllTasks(botClient, update, ct);
                    break;
                case "/report":
                    await Report(botClient, update, toDoReportService, ct);
                    break;
                case "/find":
                    await Find(botClient, update, restArgs, ct);
                    break;
            }

            OnHandleUpdateCompleted?.Invoke(update.Message.Text);
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

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"HandleError: {exception})");
        return Task.CompletedTask;
    }

    private async Task RunStart(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        await userService.RegisterUser(update.Message.From.Id, update.Message.From.Username!, ct);
        await botClient.SendMessage(update.Message.Chat, "Пользователь зарегистрирован", cancellationToken: ct);
    }

    private static async Task RunHelp(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        await botClient.SendMessage(update.Message.Chat,
            """
            Программа имитирует Telegram-чат
            Пользователю доступен набор команд...
                /start        - регистрация пользователя
                /help         - помощь
                /info         - информация о программе
            Команды доступные после регистрации пользователя:
                /addtask      - позволяет добавить задачу в список задач
                /showtasks    - позволяет просмотреть текущий список активных задач
                /removetask   - позволяет удалить задачу из списка задач по ее порядковому номеру
                /completetask - позволяет пометить задачу как завершенную по ее GUID
                /showalltasks - позволяет просмотреть список всех задач
                /report       - отчет по задачам пользователя
                /find         - поиск задач по началу их названия
            """, cancellationToken: ct);
    }

    private static async Task RunInfo(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        await botClient.SendMessage(update.Message.Chat, """
                                                         Program info: version 1.0b.
                                                         Created: Feb 18, 2025
                                                         Last updated: May 9, 2025
                                                         """, cancellationToken: ct);
    }

    private async Task AddTask(ITelegramBotClient botClient, Update update, string name, CancellationToken ct)
    {
        var user = await userService.GetUser(update.Message.From.Id, ct);

        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (user == null)
        {
            await botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован",
                cancellationToken: ct);
            return;
        }

        var newTask = await toDoService.Add(user, name, ct);
        var itemList = await toDoService.GetActiveByUserId(user.UserId, ct);
        var newTaskCountNumber = itemList.Count;
        await botClient.SendMessage(update.Message.Chat,
            $"Задача \"{newTask.Name}\" добавлена в список под номером {newTaskCountNumber}: Id = {newTask.Id}", cancellationToken: ct);
    }

    private async Task ShowTasks(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (await userService.GetUser(update.Message.From.Id, ct) == null)
        {
            await botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован",
                cancellationToken: ct);
            return;
        }

        var user = await userService.GetUser(update.Message.From.Id, ct);
        var activeTasks = await toDoService.GetActiveByUserId(user!.UserId, ct);
        var activeTaskCount = activeTasks.Count;

        await botClient.SendMessage(update.Message.Chat, "Ваш список задач:", cancellationToken: ct);
        if (activeTaskCount == 0)
            await botClient.SendMessage(update.Message.Chat, "пуст", cancellationToken: ct);
        else
        {
            var index = 1;
            foreach (var item in activeTasks.Where(t => t.State == ToDoItem.ToDoItemState.Active))
            {
                await botClient.SendMessage(update.Message.Chat,
                    $"{index++}. {item.Name} - {item.CreatedAt} - {item.Id}", cancellationToken: ct);
            }
        }
    }

    private async Task RemoveTask(ITelegramBotClient botClient, Update update, string userInputTaskToRemove,
        CancellationToken ct)
    {
        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (await userService.GetUser(update.Message.From.Id, ct) == null)
        {
            await botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован",
                cancellationToken: ct);
            return;
        }

        var user = await userService.GetUser(update.Message.From.Id, ct);
        var taskList = await toDoService.GetAllByUserId(user!.UserId, ct);
        var taskCount = taskList.Count;

        if (taskCount == 0)
        {
            await botClient.SendMessage(update.Message.Chat, "Ваш список задач пуст\nНет задач для удаления", cancellationToken: ct);

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

            await botClient.SendMessage(update.Message.Chat, "Некорректный номер задачи для удаления", cancellationToken: ct);
            await botClient.SendMessage(update.Message.Chat,
                "Введите номер задачи или нажмите q для выхода из режима удаления: ", cancellationToken: ct);
        }

        var taskToRemove = taskList[numberToRemove - 1];
        await toDoService.Delete(taskToRemove.Id, ct);
        await botClient.SendMessage(update.Message.Chat, $"Задача \"{taskToRemove.Name}\" удалена.", cancellationToken: ct);
    }

    private async Task CompleteTask(ITelegramBotClient botClient, Update update, string userInputTaskToMarkComplete,
        CancellationToken ct)
    {
        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (await userService.GetUser(update.Message.From.Id, ct) == null)
        {
            await botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован",
                cancellationToken: ct);
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
            await botClient.SendMessage(update.Message.Chat, $"Задача \"{item.Name}\" помечена исполненной.", cancellationToken: ct);
        }
    }

    private async Task ShowAllTasks(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (await userService.GetUser(update.Message.From.Id, ct) == null)
        {
            await botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован",
                cancellationToken: ct);
            return;
        }

        var user = await userService.GetUser(update.Message.From.Id, ct);
        var itemList = await toDoService.GetAllByUserId(user!.UserId, ct);

        await botClient.SendMessage(update.Message.Chat, "Ваш список задач:", cancellationToken: ct);
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

    private async Task Report(ITelegramBotClient botClient, Update update, IToDoReportService toDoReport,
        CancellationToken ct)
    {
        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (await userService.GetUser(update.Message.From.Id, ct) == null)
        {
            await botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован",
                cancellationToken: ct);
            return;
        }

        var user = await userService.GetUser(update.Message.From.Id, ct);
        var tuple = await toDoReport.GetUserStats(user!.UserId, ct);
        await botClient.SendMessage(update.Message.Chat,
            $"Статистика по задачам на {tuple.generatedAt}. Всего: {tuple.total}; Завершенных: {tuple.completed}; Активных: {tuple.active};",
            cancellationToken: ct);
    }

    private async Task Find(ITelegramBotClient botClient, Update update, string taskStartsWithString,
        CancellationToken ct)
    {
        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (await userService.GetUser(update.Message.From.Id, ct) == null)
        {
            await botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован",
                cancellationToken: ct);
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
}