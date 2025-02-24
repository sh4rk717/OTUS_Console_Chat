using System.Linq;
using System.Text.RegularExpressions;

namespace FirstInteract;

internal static class Program
{
    private static void Main(string[] args)
    {
        var userName = "";
        
        while (true)
        {
            ShowMenu(userName);

            Console.Write(userName != ""
                ? $"{userName}, введите одну из доступных команд: "
                : "Введите одну из доступных команд: ");
            
            Console.ForegroundColor = ConsoleColor.Green;
            var userInput = Console.ReadLine();
            Console.ResetColor();
            var command = "";
            if (userInput != "")
            {
                command = userInput?.Split(" ")[0]; 
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
                    RunEcho(userName, userInput);
                    break;
                case "/exit":
                case "q":
                    return;
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
        Console.WriteLine("/exit");
        if (userName != "")
            Console.WriteLine("/echo");
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void RunStart(out string userName)
    {
        do
        {
            Console.Write("Введите ваше имя: ");
            userName = Console.ReadLine();
        }
        while (!Regex.IsMatch(userName, @"^[a-zA-Zа-яА-Я]+$"));
    }

    private static void RunHelp(string userName)
    {
        if (userName != "")
            Console.WriteLine($"\n\t\tCurrent user: {userName}");
        Console.WriteLine("Программа имитирует Telegram-чат\nПользователю доступен набор команд.\n...some text...");
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

    private static void RunEcho(string userName, string userInput)
    {
        if (userName != "")
            Console.WriteLine($"\n\t\tCurrent user: {userName}");
        
        if (userInput == "/echo")
            Console.WriteLine("Программа ожидает текст после команды /echo");

        if (userInput.Length > 6)
        {
            Console.Write($"You entered: {userInput.Substring(6)}\n");
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
    
}
/*
 *Вам предстоит создать консольное приложение, которое будет имитировать интерактивное меню бота согласно следующему плану:

Приветствие: При запуске программы отображается сообщение приветствия со списком доступных команд: /start, /help, /info, /exit.
Обработка команды /start: Если пользователь вводит команду /start, программа просит его ввести своё имя. Сохраните введенное имя в переменную.
Программа должна обращаться к пользователю по имени в каждом следующем ответе.
Обработка команды /help: Отображает краткую справочную информацию о том, как пользоваться программой.
Обработка команды /info: Предоставляет информацию о версии программы и дате её создания.
Доступ к команде /echo: После ввода имени становится доступной команда /echo. При вводе этой команды с аргументом (например, /echo Hello), программа возвращает введенный текст (в данном примере "Hello").
Основной цикл программы: Программа продолжает ожидать ввод команды от пользователя, пока не будет введена команда /exit.
 * 
 */