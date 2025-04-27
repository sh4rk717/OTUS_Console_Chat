using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace FirstInteract;

internal static class Program
{
    public static int MaxTaskLength;
    public static int MaxTasks;


    public static void Main()
    {
        try
        {
            IUserService userService = new UserService();
            IToDoService toDoService = new ToDoService();
            var handler = new UpdateHandler(userService, toDoService);
            var botClient = new ConsoleBotClient();


            const int lowerTasksCount = 1;
            const int upperTasksCount = 100;
            const int lowerTaskLength = 1;
            const int upperTaskLength = 100;

            Console.Write("Введите максимально допустимое количество задач: ");
            MaxTasks = ParseAndValidateInt(Console.ReadLine(), lowerTasksCount, upperTasksCount);

            Console.Write("Введите максимально допустимую длину задачи: ");
            MaxTaskLength = ParseAndValidateInt(Console.ReadLine(), lowerTaskLength, upperTaskLength);


            // ShowMenu(botClient);
            while (true)
                botClient.StartReceiving(handler);
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