using System.Text.Json;
using FirstInteract.Core.DataAccess;
using FirstInteract.Core.Entities;

namespace FirstInteract.Infrastructure.DataAccess;

public class FileUserRepository : IUserRepository
{
    private readonly string _path;

    //для записи в JSON-файл с отступами
    private static readonly JsonSerializerOptions SWriteOptions = new()
    {
        WriteIndented = true
    };

    public FileUserRepository(string path)
    {
        _path = path;
        //создаем директорию если нет
        Directory.CreateDirectory(_path);
    }

    public async Task<ToDoUser?>? GetUser(Guid userId, CancellationToken ct)
    {
        var fullPath = Path.Combine(this._path, userId + ".json");
        if (!File.Exists(fullPath))
            return null;

        // Читаем JSON из файла
        var json = await File.ReadAllTextAsync(fullPath, ct);
        // Десериализуем item из файла
        var user = JsonSerializer.Deserialize<ToDoUser>(json);

        return user;
    }

    public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
    {
        // Получаем все файлы пользователей в директории
        var userFiles = Directory.GetFiles(_path, "*.json");
    
        foreach (var filePath in userFiles)
        {
            try
            {
                // Читаем JSON из файла
                var json = await File.ReadAllTextAsync(filePath, ct);
                // Десериализуем пользователя
                var user = JsonSerializer.Deserialize<ToDoUser>(json);
            
                // Проверяем совпадение Telegram User ID
                if (user != null && user.TelegramUserId == telegramUserId)
                {
                    return user; // Нашли нужного пользователя
                }
            }
            catch (JsonException)
            {
                // Пропускаем некорректные JSON-файлы
            }
            catch (IOException)
            {
                // Пропускаем файлы с ошибками чтения
            }
        }
    
        return null; // Пользователь не найден
    }

    public async Task Add(ToDoUser user, CancellationToken ct)
    {
        // Пользователь уже зарегистрирован
        if (await GetUserByTelegramUserId(user.TelegramUserId, ct) != null)
            return;
        
        var fullPath = Path.Combine(this._path, user.UserId + ".json");
        //Создание файла
        File.Create(fullPath).Close();
        // Сериализация в JSON
        var itemJson = JsonSerializer.Serialize(user, SWriteOptions);
        // Запись в файл
        await File.WriteAllTextAsync(fullPath, itemJson, ct);

        Console.WriteLine($"{user.TelegramUserName} - успешно сериализовано и записано в файл.");

        // _users.Add(user); // для теста пока оставил
        // return Task.CompletedTask;
    }
}