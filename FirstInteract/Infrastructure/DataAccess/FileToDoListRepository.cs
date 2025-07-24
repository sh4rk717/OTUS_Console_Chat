using System.Text.Json;
using FirstInteract.Core.DataAccess;
using FirstInteract.Core.Entities;

namespace FirstInteract.Infrastructure.DataAccess;

public class FileToDoListRepository : IToDoListRepository
{
    private readonly string _path;

    //для записи в JSON-файл с отступами
    private static readonly JsonSerializerOptions SWriteOptions = new()
    {
        WriteIndented = true
    };

    public FileToDoListRepository(string path)
    {
        _path = path;
        //создаем директорию если нет
        Directory.CreateDirectory(_path);
    }
    
    
    public async Task<ToDoList?>? Get(Guid id, CancellationToken ct)
    {
        var fullPath = Path.Combine(this._path, id + ".json");
        if (!File.Exists(fullPath))
            return null;

        // Читаем JSON из файла
        var json = await File.ReadAllTextAsync(fullPath, ct);
        // Десериализуем item из файла
        var toDoList = JsonSerializer.Deserialize<ToDoList>(json);

        return toDoList;
    }

    public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
    {
        var toDoLists = new List<ToDoList>();
        
        // Получаем все файлы списков (toDoList) в директории
        var toDoListFiles = Directory.GetFiles(_path, "*.json");
    
        foreach (var filePath in toDoListFiles)
        {
            try
            {
                // Читаем JSON из файла
                var json = await File.ReadAllTextAsync(filePath, ct);
                // Десериализуем отдельный список задач
                var toDoList = JsonSerializer.Deserialize<ToDoList>(json);
            
                // Проверяем совпадение User ID
                if (toDoList!.User.UserId == userId)
                {
                    toDoLists.Add(toDoList); // Добавляем значение в возвращаемый список
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
    
        return toDoLists;
    }

    public async Task Add(ToDoList list, CancellationToken ct)
    {
        var fullPath = Path.Combine(_path, list.Id + ".json");
        //Создание файла
        File.Create(fullPath).Close();
        // Сериализация в JSON
        var listJson = JsonSerializer.Serialize(list, SWriteOptions);
        // Запись в файл
        await File.WriteAllTextAsync(fullPath, listJson, ct);

        Console.WriteLine($"{list.Name} - Список задач: успешно сериализовано и записано в файл.");
    }

    public Task Delete(Guid id, CancellationToken ct)
    {
        var fullPath = Path.Combine(_path, id + ".json");
        File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
    {
        // Получаем все файлы списков (toDoList) в директории
        var toDoListFiles = Directory.GetFiles(_path, "*.json");
    
        foreach (var filePath in toDoListFiles)
        {
            try
            {
                // Читаем JSON из файла
                var json = await File.ReadAllTextAsync(filePath, ct);
                // Десериализуем отдельный список задач
                var toDoList = JsonSerializer.Deserialize<ToDoList>(json);
            
                // Проверяем совпадение User ID и названия списка
                if (toDoList!.User.UserId == userId && toDoList.Name == name)
                {
                    return true;
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
    
        return false;
    }
}