using System.Text;
using FirstInteract.Core.DataAccess;
using FirstInteract.Core.Services;
using FirstInteract.Infrastructure.DataAccess;
using FirstInteract.TelegramBot;
using FirstInteract.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace FirstInteract;

internal static class Program
{
    public static int MaxTaskLength;
    public static int MaxTasks;

    public static async Task Main()
    {
        Console.InputEncoding = Encoding.Default;
        Console.OutputEncoding = Encoding.Default;
        
        IUserRepository userFileRepository = new FileUserRepository(Path.Combine("..", "..", "..", "users"));
        IToDoRepository toDoFileRepository = new FileToDoRepository(Path.Combine("..", "..", "..", "items"));
        IUserService userService = new UserService(userFileRepository);
        IToDoService toDoService = new ToDoService(toDoFileRepository);
        IToDoReportService toDoReportService = new ToDoReportService(toDoFileRepository);
        IEnumerable<IScenario> scenarios = [new AddTaskScenario(userService, toDoService)];
        IScenarioContextRepository contextRepository = new InMemoryScenarioContextRepository();

        //Linux env. var
        var token = Environment.GetEnvironmentVariable("Telegram_TOKEN");
        
        if (string.IsNullOrEmpty(token))
            //Windows env. var
            token = Environment.GetEnvironmentVariable("Telegram_TOKEN", EnvironmentVariableTarget.User);
        
        if (string.IsNullOrEmpty(token))
            throw new InvalidOperationException(
                "Telegram bot token is not configured. Please set the Telegram_TOKEN environment variable.");

        var botClient = new TelegramBotClient(token);
        var handler = new UpdateHandler(botClient, userService, toDoService, toDoReportService, scenarios,
            contextRepository);
        try
        {
            using var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = [UpdateType.Message],
                DropPendingUpdates = true
            };

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

            botClient.StartReceiving(handler, receiverOptions, cancellationToken: cts.Token);

            var me = await botClient.GetMe(cancellationToken: cts.Token);
            Console.WriteLine($"{me.FirstName} запущен!");

            Console.WriteLine("Нажмите клавишу A для выхода");
            while (!cts.Token.IsCancellationRequested)
            {
                var symbol = Console.ReadKey();
                if (symbol.Key == ConsoleKey.A)
                {
                    await cts.CancelAsync();
                    Console.WriteLine("\nBot stopped");
                    break;
                }

                Console.WriteLine($"\n{me.Id} - {me.FirstName} - {me.LastName} - {me.Username}");
            }
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

    private static void DisplayMessageStart(string message)
    {
        Console.WriteLine($"Началась обработка сообщения '{message}' в {DateTime.Now:O}");
    }

    private static void DisplayMessageStop(string message)
    {
        Console.WriteLine($"Закончилась обработка сообщения '{message}' в {DateTime.Now:O}");
    }
}