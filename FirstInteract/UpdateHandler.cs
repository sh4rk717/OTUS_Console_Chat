using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace FirstInteract;

public class UpdateHandler(IUserService userService, IToDoService toDoService) : IUpdateHandler
{
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

    private static void ShowMenu(ITelegramBotClient botClient, Update update)
    {
        foreach (var command in Db.CommandsList)
        {
            botClient.SendMessage(update.Message.Chat, command);
        }

        botClient.SendMessage(update.Message.Chat, "Введите одну из доступных команд: ");
    }

    private void RunStart(ITelegramBotClient botClient, Update update)
    {
        userService.RegisterUser(update.Message.From.Id, update.Message.From.Username!);
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
        var user = userService.GetUser(update.Message.From.Id);

        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (user == null)
        {
            botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован");
            return;
        }

        var newTask = toDoService.Add(user, name);
        var newTaskCountNumber = toDoService.GetAllByUserId(user.UserId).Count;
        botClient.SendMessage(update.Message.Chat,
            $"Задача \"{newTask.Name}\" добавлена в список под номером {newTaskCountNumber}: Id = {newTask.Id}");
    }

    private void ShowTasks(ITelegramBotClient botClient, Update update)
    {
        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (userService.GetUser(update.Message.From.Id) == null)
        {
            botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован");
            return;
        }

        var user = userService.GetUser(update.Message.From.Id);
        var activeTaskCount = toDoService.GetActiveByUserId(user!.UserId).Count;
        
        botClient.SendMessage(update.Message.Chat, "Ваш список задач:");
        if (activeTaskCount == 0) botClient.SendMessage(update.Message.Chat, "пуст");
        else
        {
            var index = 1;
            foreach (var task in toDoService.GetActiveByUserId(user.UserId).Where(t => t.State == ToDoItem.ToDoItemState.Active))
            {
                botClient.SendMessage(update.Message.Chat,
                    $"{index++}. {task.Name} - {task.CreatedAt} - {task.Id}");
            }
        }
    }

    private void RemoveTask(ITelegramBotClient botClient, Update update, string userInputTaskToRemove)
    {
        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (userService.GetUser(update.Message.From.Id) == null)
        {
            botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован");
            return;
        }
        
        var user = userService.GetUser(update.Message.From.Id);
        var taskList = toDoService.GetAllByUserId(user!.UserId);
        var taskCount = toDoService.GetAllByUserId(user.UserId).Count;

        if (taskCount == 0)
        {
            botClient.SendMessage(update.Message.Chat, "Ваш список задач пуст\nНет задач для удаления");

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

            botClient.SendMessage(update.Message.Chat, "Некорректный номер задачи для удаления");
            botClient.SendMessage(update.Message.Chat,
                "Введите номер задачи или нажмите q для выхода из режима удаления: ");
        }

        var taskToRemove = taskList[numberToRemove - 1];
        toDoService.Delete(taskToRemove.Id);
        botClient.SendMessage(update.Message.Chat, $"Задача \"{taskToRemove.Name}\" удалена.");
    }

    private void CompleteTask(ITelegramBotClient botClient, Update update, string userInputTaskToMarkComplete)
    {
        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (userService.GetUser(update.Message.From.Id) == null)
        {
            botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован");
            return;
        }

        var user = userService.GetUser(update.Message.From.Id);
        var taskList = toDoService.GetActiveByUserId(user!.UserId);
        
        if (taskList.Count == 0)
        {
            botClient.SendMessage(update.Message.Chat,
                "Ваш список задач пуст\nНет задач чтоб пометить их как выполненные");
            return;
        }

        foreach (var task in taskList.Where(x => x.Id == Guid.Parse(userInputTaskToMarkComplete)))
        {
            toDoService.MarkCompleted(task.Id);
            botClient.SendMessage(update.Message.Chat, $"Задача \"{task.Name}\" помечена исполненной.");
        }
    }

    private void ShowAllTasks(ITelegramBotClient botClient, Update update)
    {
        //если пользователь не зарегистрирован, то ничего не происходит при вызове
        if (userService.GetUser(update.Message.From.Id) == null)
        {
            botClient.SendMessage(update.Message.Chat, "Команда не доступна. Пользователь не зарегистрирован");
            return;
        }
        
        var user = userService.GetUser(update.Message.From.Id);
        var taskList = toDoService.GetAllByUserId(user!.UserId);
        
        botClient.SendMessage(update.Message.Chat, "Ваш список задач:");
        if (taskList.Count == 0) botClient.SendMessage(update.Message.Chat, "пуст");
        else
        {
            var index = 1;
            foreach (var task in taskList)
            {
                botClient.SendMessage(update.Message.Chat,
                    $"({task.State}) {index++}. {task.Name} - {task.CreatedAt} - {task.Id}");
            }
        }
    }
}