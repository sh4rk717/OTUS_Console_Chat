namespace FirstInteract;

internal static class Program
{
    private static void Main()
    {
        var userName = "";
        List<string> tasks = [];

        while (true)
        {
            try
            {
                Console.Write("Введите максимально допустимое количество задач: ");
                var maxTasks = ParseAndValidateInt(Console.ReadLine(), 1, 100);

                Console.Write("Введите максимально допустимую длину задачи: ");
                var maxTaskLength = ParseAndValidateInt(Console.ReadLine(), 1, 100);

                while (true)
                {
                    try
                    {
                        // throw new Exception();
                        // throw new ArgumentException();
                        // throw new TaskCountLimitException(5);

                        ShowMenu(userName);

                        Console.Write(userName != ""
                            ? $"Привет, {userName}! Введите одну из доступных команд: "
                            : "Введите одну из доступных команд: ");

                        Console.ForegroundColor = ConsoleColor.Green;
                        var userInputCommand = ValidateString(Console.ReadLine());
                        Console.ResetColor();
                        var command = "";
                        if (userInputCommand != "")
                        {
                            command = userInputCommand.Split(" ")[0];
                        }

                        switch (command)
                        {
                            case "/start":
                                RunStart(out userName);
                                break;
                            case "/help":
                                RunHelp(userName);
                                break;
                            case "/info":
                                RunInfo(userName);
                                break;
                            case "/echo":
                                RunEcho(userName, userInputCommand);
                                break;
                            case "/addtask":
                            case "/at":
                                AddTask(tasks, maxTasks, maxTaskLength);
                                break;
                            case "/showtasks":
                            case "/st":
                                ShowTasks(tasks);
                                break;
                            case "/removetask":
                            case "/rt":
                                RemoveTask(tasks);
                                break;
                            case "/exit":
                            case "q":
                                return;
                        }
                    }
                    catch (ArgumentException e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e.Message);
                        Console.ResetColor();
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                    }
                    catch (TaskCountLimitException e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e.Message);
                        Console.ResetColor();
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                    }
                    catch (TaskLengthLimitException e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e.Message);
                        Console.ResetColor();
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                    }
                    catch (DuplicateTaskException e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e.Message);
                        Console.ResetColor();
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Произошла непредвиденная ошибка: ");
                        Console.WriteLine(
                            $"e.GetType = {e.GetType()}\ne.Message = {e.Message}\ne.StackTrace = {e.StackTrace}\ne.InnerException = {e.InnerException}");
                        Console.ResetColor();
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                    }
                }
            }
            catch (ArgumentException e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(e.Message);
                Console.ResetColor();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }

    private static void ShowMenu(string userName = "")
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("/start");
        Console.WriteLine("/help");
        Console.WriteLine("/info");
        Console.WriteLine("/exit or q");
        if (userName != "")
        {
            Console.WriteLine("/echo");
            Console.WriteLine("/addtask or /at");
            Console.WriteLine("/showtasks or /st");
            Console.WriteLine("/removetask or /rt");
        }

        Console.ResetColor();
    }

    private static void RunStart(out string userName)
    {
        do
        {
            Console.Write("Пожалуйста, введите ваше имя: ");
            userName = ValidateString(Console.ReadLine());
        } while (userName.Length < 2);
    }

    private static void RunHelp(string userName)
    {
        if (userName != "")
            Console.WriteLine($"\n\t\tCurrent user: {userName}");
        Console.WriteLine("Программа имитирует Telegram-чат\nПользователю доступен набор команд...\n...some text...");
        Console.WriteLine("Команды доступные в чате после ввода имени пользователя:");
        Console.WriteLine("\t/echo       - печатает в ответ переданный ей текст;");
        Console.WriteLine("\t/addtask    - позволяет добавить задачу в список задач;");
        Console.WriteLine("\t/showtasks  - позволяет просмотреть текущий список задач;");
        Console.WriteLine("\t/removetask - позволяет удалить задачу из списка задач");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private static void RunInfo(string userName)
    {
        if (userName != "")
            Console.WriteLine($"\n\t\tCurrent user: {userName}");
        Console.WriteLine("Program info: version 1.0b.\nCreated: Feb 18, 2025");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private static void RunEcho(string userName, string? userInput)
    {
        if (userName != "")
            Console.WriteLine($"\n\t\tCurrent user: {userName}");

        if (userInput == "/echo")
            Console.WriteLine("Программа ожидает текст после команды /echo");

        if (userInput is { Length: > 6 })
        {
            Console.WriteLine($"You entered: {userInput[6..]}");
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private static void AddTask(List<string> tasks, int maxTasks, int maxTaskLength)
    {
        if (tasks.Count >= maxTasks)
            throw new TaskCountLimitException(maxTasks);
        Console.Write("Введите описание задачи: ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        var newTask = ValidateString(Console.ReadLine());
        Console.ResetColor();
        if (newTask.Length > maxTaskLength)
            throw new TaskLengthLimitException(newTask.Length, maxTaskLength);
        if (tasks.Contains(newTask))
            throw new DuplicateTaskException(newTask);
        tasks.Add(newTask);
        Console.WriteLine($"Задача \"{newTask}\" добавлена в список под номером {tasks.Count}");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private static void ShowTasks(List<string> tasks)
    {
        Console.Write("Ваш список задач: ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        if (tasks.Count == 0) Console.WriteLine("пуст");
        else
        {
            Console.WriteLine();
            for (int i = 0; i < tasks.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {tasks[i]}");
            }
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private static void RemoveTask(List<string> tasks)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        if (tasks.Count == 0)
        {
            Console.WriteLine("Ваш список задач пуст\nНет задач для удаления");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("Вот ваш список задач: ");
        for (var i = 0; i < tasks.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {tasks[i]}");
        }

        Console.Write("Введите номер задачи для удаления: ");
        var numberToRemove = 0;
        var isInt = false;
        while (numberToRemove <= 0 || numberToRemove > tasks.Count || !isInt)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            var userInputTaskToRemove = ValidateString(Console.ReadLine());
            if (userInputTaskToRemove == "q") return;
            isInt = int.TryParse(userInputTaskToRemove, out numberToRemove);
            if (numberToRemove > 0 && numberToRemove <= tasks.Count && isInt) continue;
            Console.WriteLine("Некорректный номер задачи для удаления");
            Console.Write("Введите номер задачи или нажмите q для выхода из режима удаления: ");
        }

        var taskToRemove = tasks[numberToRemove - 1];
        tasks.RemoveAt(numberToRemove - 1);
        Console.WriteLine($"Задача \"{taskToRemove}\" удалена.");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private static int ParseAndValidateInt(string? str, int min, int max)
    {
        var isNumber = int.TryParse(str, out var number);
        if (!isNumber || number < min || number > max)
            throw new ArgumentException();
        return number;
    }

    private static string ValidateString(string? str)
    {
        if (str != null && str.Replace(" ", "").Replace("\t", "").Length > 0)
            return str;
        throw new ArgumentException();
    }
}