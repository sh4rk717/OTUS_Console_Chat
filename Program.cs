namespace FirstInteract;

internal static class Program
{
	private static void Main()
	{
		var userName = "";
		
		while (true)
		{
			ShowMenu(userName);

			Console.Write(userName != ""
				? $"{userName}, введите одну из доступных команд: "
				: "Введите одну из доступных команд: ");
			
			Console.ForegroundColor = ConsoleColor.Green;
			var userInput = Console.ReadLine() ?? throw new NullReferenceException();
			Console.ResetColor();
			var command = "";
			if (userInput != "")
			{
				command = userInput.Split(" ")[0]; 
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
			userName = Console.ReadLine() ?? throw new NullReferenceException();
		}
		while (userName.Length < 2);
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
	
}
