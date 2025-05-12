using FirstInteract.Core.DataAccess;
using FirstInteract.Core.Services;
using FirstInteract.Infrastructure.DataAccess;
using FirstInteract.TelegramBot;
using Otus.ToDoList.ConsoleBot;

namespace FirstInteract;

internal static class Program
{
    public static int MaxTaskLength;
    public static int MaxTasks;

    private static void DisplayMessageStart(string message)
    {
        Console.WriteLine($"Началась обработка сообщения '{message}' в {DateTime.Now}");
    }

    private static void DisplayMessageStop(string message)
    {
        Console.WriteLine($"Закончилась обработка сообщения '{message}' в {DateTime.Now}");
    }

    public static void Main()
    {
        IUserRepository userRepository = new InMemoryUserRepository();
        IToDoRepository toDoRepository = new InMemoryToDoRepository();
        IToDoReportService toDoReportService = new ToDoReportService(toDoRepository);
        IUserService userService = new UserService(userRepository);
        IToDoService toDoService = new ToDoService(toDoRepository);
        var handler = new UpdateHandler(userService, toDoService, toDoReportService);

        try
        {
            var botClient = new ConsoleBotClient();
            using var cts = new CancellationTokenSource();
            
            // подписка на события
            handler.OnHandleUpdateStarted += DisplayMessageStart;
            handler.OnHandleUpdateCompleted += DisplayMessageStop;

            const int lowerTasksCount = 1;
            const int upperTasksCount = 100;
            const int lowerTaskLength = 1;
            const int upperTaskLength = 100;

            Console.Write("Введите максимально допустимое количество задач: ");
            MaxTasks = ParseAndValidateInt(Console.ReadLine(), lowerTasksCount, upperTasksCount);

            Console.Write("Введите максимально допустимую длину задачи: ");
            MaxTaskLength = ParseAndValidateInt(Console.ReadLine(), lowerTaskLength, upperTaskLength);
            
            while (true)
                botClient.StartReceiving(handler, cts.Token);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Произошла непредвиденная ошибка: ");
            Console.WriteLine($"e.GetType = {e.GetType()}\n" +
                              $"e.Message = {e.Message}\n" +
                              $"e.StackTrace = {e.StackTrace}\n" +
                              $"e.InnerException = {e.InnerException}");
            Console.ResetColor();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        finally
        {
            // отписка от событий
            handler.OnHandleUpdateStarted -= DisplayMessageStart;
            handler.OnHandleUpdateCompleted -= DisplayMessageStop;
        }
    }

    private static int ParseAndValidateInt(string? str, int min, int max)
    {
        var isNumber = int.TryParse(str, out var number);
        if (!isNumber || number < min || number > max)
            throw new ArgumentException(
                $"Некорректное значение. Ожидается целое число из диапазона [{min}, {max}]");
        return number;
    }

    public static string ValidateString(string? str)
    {
        if (string.IsNullOrWhiteSpace(str)) throw new ArgumentException("Некорректная строка");

        return str;
    }
}