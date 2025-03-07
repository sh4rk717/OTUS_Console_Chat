namespace FirstInteract;

internal static class Program
{
	private static void Main()
	{
		var userName = "";
		List<String> tasks = new List<String>();
		
		while (true)
		{
			ShowMenu(userName);

			Console.Write(userName != ""
				? $"Привет, {userName}! Введите одну из доступных команд: "
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
				case "/addtask":
				case "/at":
					AddTask(tasks);
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
			userName = Console.ReadLine() ?? throw new NullReferenceException();
		}
		while (userName.Length < 2);
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
	
	private static void AddTask(List<string> tasks)
	{
		Console.Write("Введите описание задачи: ");
		Console.ForegroundColor = ConsoleColor.Yellow;
		var newTask = Console.ReadLine();
		Console.ResetColor();
		if (newTask != null) tasks.Add(newTask);
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
		for (int i = 0; i < tasks.Count; i++)
		{
			Console.WriteLine($"{i + 1}. {tasks[i]}");
		}

		Console.Write("Введите номер задачи для удаления: ");
		int numberToRemove = 0;
		var isInt = false;
		while (numberToRemove <= 0 || numberToRemove > tasks.Count || !isInt)
		{
				Console.ForegroundColor = ConsoleColor.Yellow;
				var userInput = Console.ReadLine();
				if (userInput == "q") return;
				isInt = int.TryParse(userInput, out numberToRemove);
				if (numberToRemove <= 0 || numberToRemove > tasks.Count || !isInt)
				{
					Console.WriteLine("Некорректный номер задачи для удаления");
					Console.Write("Введите номер задачи или нажмите q для выхода из режима удаления: ");
				}
		}
		var taskToRemove = tasks[numberToRemove - 1];
		tasks.RemoveAt(numberToRemove - 1);
		Console.WriteLine($"Задача \"{taskToRemove}\" удалена.");
		Console.WriteLine("Press any key to continue...");
		Console.ReadKey();
	}
}
