using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace FirstInteract;

public class UpdateHandler : IUpdateHandler
{
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;

    public UpdateHandler(IUserService userService, IToDoService toDoService)
    {
        _userService = userService;
        _toDoService = toDoService;
    }

    public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
    {
        try
        {
            // ShowMenu(botClient, update);

            var command = update.Message.Text.Split(" ")[0]; //до 1-ого пробела
            var restArgs = string.Join(" ", update.Message.Text.Split(" ")[1..]); //все остальное после 1-ого пробела
            switch (command)
            {
                case "/start":
                    RunStart(botClient, update);
                    ShowMenu(botClient, update);
                    break;
                case "/help":
                    RunHelp(botClient, update);
                    ShowMenu(botClient, update);
                    break;
                case "/info":
                    RunInfo(botClient, update);
                    ShowMenu(botClient, update);
                    break;
                case "/addtask":
                    AddTask(botClient, update, restArgs);
                    // ShowMenu(botClient, update);
                    break;
                case "/showtasks":
                    ShowTasks(botClient, update);
                    ShowMenu(botClient, update);
                    break;
                case "/removetask":
                    RemoveTask(botClient, update, restArgs);
                    ShowMenu(botClient, update);
                    break;
                case "/completetask":
                    CompleteTask(botClient, update, restArgs);
                    ShowMenu(botClient, update);
                    break;
                case "/showalltasks":
                    ShowAllTasks(botClient, update);
                    ShowMenu(botClient, update);
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

    private void ShowMenu(ITelegramBotClient botClient, Update update)
    {
        var user = _userService.GetUser(update.Message.From.Id);

        foreach (var command in Db.CommandsList)
        {
            botClient.SendMessage(update.Message.Chat, command);
        }

        botClient.SendMessage(update.Message.Chat, "Введите одну из доступных команд: ");
    }

    private void RunStart(ITelegramBotClient botClient, Update update)
    {
        _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username!);
        botClient.SendMessage(update.Message.Chat, "Пользователь зарегистрирован");
    }

    private static void RunHelp(ITelegramBotClient botClient, Update update)
    {
        botClient.SendMessage(update.Message.Chat,
            """
            Программа имитирует Telegram-чат
            Пользователю доступен набор команд...
            ...some text...
            Команды доступные в чате после регистрации пользователя:
                /addtask    - позволяет добавить задачу в список задач
                /showtasks  - позволяет просмотреть текущий список активных задач
                /removetask - позволяет удалить задачу из списка задач
                /completetask - позволяет пометить задачу как завершенную
                /showalltasks - позволяет просмотреть список всех задач
            """);
    }

    private static void RunInfo(ITelegramBotClient botClient, Update update)
    {
        botClient.SendMessage(update.Message.Chat, """
                                                   Program info: version 1.0b.
                                                   Created: Feb 18, 2025
                                                   Last updated: Apr 27, 2025
                                                   """);
    }

    private void AddTask(ITelegramBotClient botClient, Update update, string name)
    {
        var user = _userService.GetUser(update.Message.From.Id);

        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (user == null)
        {
            botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован");
            return;
        }

        var newTask = _toDoService.Add(user, name);
        botClient.SendMessage(update.Message.Chat,
            $"Задача \"{newTask.Name}\" добавлена в список под номером {Db.TasksList.Count}: Id = {newTask.Id}");
    }

    private void ShowTasks(ITelegramBotClient botClient, Update update)
    {
        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (_userService.GetUser(update.Message.From.Id) == null)
        {
            botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован");
            return;
        }

        botClient.SendMessage(update.Message.Chat, "Ваш список задач:");
        if (Db.TasksList.Count == 0) botClient.SendMessage(update.Message.Chat, "пуст");
        else
        {
            var index = 1;
            foreach (var task in Db.TasksList.Where(t => t.State == ToDoItem.ToDoItemState.Active))
            {
                botClient.SendMessage(update.Message.Chat,
                    $"{index++}. {task.Name} - {task.CreatedAt} - {task.Id}");
            }
        }
    }

    private void RemoveTask(ITelegramBotClient botClient, Update update, string userInputTaskToRemove)
    {
        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (_userService.GetUser(update.Message.From.Id) == null)
        {
            botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован");
            return;
        }

        if (Db.TasksList.Count == 0)
        {
            botClient.SendMessage(update.Message.Chat, "Ваш список задач пуст\nНет задач для удаления");

            return;
        }

        var numberToRemove = 0;
        var isInt = false;
        while (numberToRemove <= 0 || numberToRemove > Db.TasksList.Count || !isInt)
        {
            userInputTaskToRemove = Program.ValidateString(userInputTaskToRemove);
            if (userInputTaskToRemove == "q") return;

            isInt = int.TryParse(userInputTaskToRemove, out numberToRemove);

            if (numberToRemove > 0 && numberToRemove <= Db.TasksList.Count && isInt) continue;

            botClient.SendMessage(update.Message.Chat, "Некорректный номер задачи для удаления");
            botClient.SendMessage(update.Message.Chat,
                "Введите номер задачи или нажмите q для выхода из режима удаления: ");
        }

        var taskToRemove = Db.TasksList[numberToRemove - 1];
        Db.TasksList.RemoveAt(numberToRemove - 1);
        botClient.SendMessage(update.Message.Chat, $"Задача \"{taskToRemove.Name}\" удалена.");
    }

    private void CompleteTask(ITelegramBotClient botClient, Update update, string userInputTaskToMarkComplete)
    {
        var service = _toDoService;

        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (_userService.GetUser(update.Message.From.Id) == null)
        {
            botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован");
            return;
        }

        if (Db.TasksList.Count == 0)
        {
            botClient.SendMessage(update.Message.Chat,
                "Ваш список задач пуст\nНет задач чтоб пометить их как выполненные");
            return;
        }

        foreach (var task in Db.TasksList.Where(x => x.Id == Guid.Parse(userInputTaskToMarkComplete)))
        {
            service.MarkCompleted(task.Id);
            botClient.SendMessage(update.Message.Chat, $"Задача \"{task.Name}\" помечена исполненной.");
        }
    }

    private void ShowAllTasks(ITelegramBotClient botClient, Update update)
    {
        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (_userService.GetUser(update.Message.From.Id) == null)
        {
            botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован");
            return;
        }

        botClient.SendMessage(update.Message.Chat, "Ваш список задач:");
        if (Db.TasksList.Count == 0) botClient.SendMessage(update.Message.Chat, "пуст");
        else
        {
            var index = 1;
            foreach (var task in Db.TasksList)
            {
                botClient.SendMessage(update.Message.Chat,
                    $"({task.State}) {index++}. {task.Name} - {task.CreatedAt} - {task.Id}");
            }
        }
    }
}