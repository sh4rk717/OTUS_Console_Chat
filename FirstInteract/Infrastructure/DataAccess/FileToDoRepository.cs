using System.Text.Json;
using System.Text.Json.Nodes;
using FirstInteract.Core.DataAccess;
using FirstInteract.Core.Entities;

namespace FirstInteract.Infrastructure.DataAccess;

public class FileToDoRepository : IToDoRepository
{
    private readonly string _path;

    //для записи в JSON-файл с отступами
    private static readonly JsonSerializerOptions SWriteOptions = new()
    {
        WriteIndented = true
    };

    public FileToDoRepository(string path)
    {
        _path = path;
        Directory.CreateDirectory(_path); // создаем директорию если нет

        var fullIndexPath = Path.Combine(_path, "index.json");

        // Если файл индекса не существует или пустой
        if (!File.Exists(fullIndexPath) || new FileInfo(fullIndexPath).Length == 0)
        {
            // Создаем/очищаем файл
            File.WriteAllText(fullIndexPath, "{}");

            // Сканируем существующие задачи и строим индекс
            RebuildIndex();
        }
        else
        {
            try
            {
                // Проверяем валидность существующего индекса
                var json = File.ReadAllText(fullIndexPath);
                JsonSerializer.Deserialize<Dictionary<Guid, List<Guid>>>(json);
            }
            catch (JsonException)
            {
                // Если индекс поврежден - перестраиваем
                RebuildIndex();
            }
        }
    }

    public Task Add(ToDoItem item, CancellationToken ct)
    {
        // Создаем директорию пользователя, если не существует
        var userDir = Path.Combine(_path, item.User.UserId.ToString());
        Directory.CreateDirectory(userDir);

        // Путь к файлу задачи
        var fullPath = Path.Combine(userDir, item.Id + ".json");

        // Сериализуем и сохраняем задачу
        var itemJson = JsonSerializer.Serialize(item, SWriteOptions);
        File.WriteAllText(fullPath, itemJson);

        // Обновляем индекс
        UpdateIndex(item.User.UserId, item.Id, "add");

        Console.WriteLine($"{item.Name} - успешно сохранено (ID: {item.Id})");
        return Task.CompletedTask;
    }

    public async Task<ToDoItem?> Get(Guid id, CancellationToken ct)
    {
        // Ищем во всех подпапках пользователей
        var userDirectories = Directory.GetDirectories(_path);

        foreach (var userDir in userDirectories)
        {
            var fullPath = Path.Combine(userDir, id + ".json");

            if (!File.Exists(fullPath))
                continue;

            try
            {
                // Читаем JSON из файла
                var json = await File.ReadAllTextAsync(fullPath, ct);
                // Десериализуем item из файла
                var item = JsonSerializer.Deserialize<ToDoItem>(json);

                return item;
            }
            catch (JsonException)
            {
                // Пропускаем битые файлы
            }
            catch (IOException)
            {
                // Пропускаем файлы с ошибками доступа
            }
        }

        return null; // Не нашли ни в одной папке
    }

    public async Task Update(ToDoItem item, CancellationToken ct)
    {
        var fullPath = Path.Combine(_path, item.User.UserId.ToString(), item.Id + ".json");

        // Проверяем, существует ли файл
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Файл с id {item.Id} не найден.");
        // Сериализуем обновлённый объект в JSON
        var updatedItemJson = JsonSerializer.Serialize(item, SWriteOptions);
        // Перезаписываем файл
        await File.WriteAllTextAsync(fullPath, updatedItemJson, ct);

        Console.WriteLine($"{item.Name} - успешно обновлён.");
    }

    public Task Delete(Guid itemId, CancellationToken ct)
    {
        // 1. Загружаем текущий индекс
        var indexData = LoadIndex();
        Guid? userId = null;

        // 2. Ищем пользователя, которому принадлежит задача
        foreach (var userEntry in indexData.UserToItems)
        {
            if (userEntry.Value.Contains(itemId))
            {
                userId = userEntry.Key;
                break;
            }
        }

        if (userId == null)
        {
            Console.WriteLine($"Задача с id {itemId} не найдена в индексе");
            return Task.CompletedTask;
        }

        // 3. Формируем путь к файлу
        var itemPath = Path.Combine(_path, userId.Value.ToString(), $"{itemId}.json");

        if (!File.Exists(itemPath))
        {
            Console.WriteLine($"Файл задачи {itemId} не найден по ожидаемому пути: {itemPath}");
            RebuildIndex(); // Восстанавливаем целостность индекса
            return Task.CompletedTask;
        }

        try
        {
            // 4. Удаляем файл задачи
            File.Delete(itemPath);
        
            // 5. Обновляем индекс
            indexData.UserToItems[userId.Value].Remove(itemId);
            SaveIndex(indexData);
        
            Console.WriteLine($"Задача {itemId} пользователя {userId} успешно удалена");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Ошибка при удалении: {ex.Message}");
            throw;
        }

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
    {
        var fullPath = Path.Combine(_path, userId.ToString());
        //Создаем директорию, если не существует
        Directory.CreateDirectory(fullPath);

        // Получаем все JSON-файлы в директории
        var jsonFiles = Directory.GetFiles(fullPath, "*.json");

        var allUserItems = new List<ToDoItem>();

        foreach (var filePath in jsonFiles)
        {
            // Читаем JSON из файла
            var json = await File.ReadAllTextAsync(filePath, ct);
            // Десериализуем элемент
            var item = JsonSerializer.Deserialize<ToDoItem>(json);

            // Проверяем условия: пользователь и активный статус
            if (item != null &&
                item.User.UserId == userId)
            {
                allUserItems.Add(item);
            }
        }

        return allUserItems;
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
    {
        var fullPath = Path.Combine(_path, userId.ToString());
        //Создаем директорию, если не существует
        Directory.CreateDirectory(fullPath);

        // Получаем все JSON-файлы в директории
        var jsonFiles = Directory.GetFiles(fullPath, "*.json");

        var activeItems = new List<ToDoItem>();

        foreach (var filePath in jsonFiles)
        {
            // Читаем JSON из файла
            var json = await File.ReadAllTextAsync(filePath, ct);
            // Десериализуем элемент
            var item = JsonSerializer.Deserialize<ToDoItem>(json);

            // Проверяем условия: пользователь и активный статус
            if (item != null &&
                item.User.UserId == userId &&
                item.State == ToDoItem.ToDoItemState.Active)
            {
                activeItems.Add(item);
            }
        }

        return activeItems;
    }

    public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
    {
        var fullPath = Path.Combine(_path, userId.ToString());
        // Получаем все JSON-файлы в директории
        var jsonFiles = Directory.GetFiles(fullPath, "*.json");

        foreach (var filePath in jsonFiles)
        {
            try
            {
                // Читаем и десериализуем задачу
                var json = await File.ReadAllTextAsync(filePath, ct);
                var item = JsonSerializer.Deserialize<ToDoItem>(json);

                // Проверяем совпадение пользователя и имени
                if (item != null &&
                    item.User.UserId == userId &&
                    string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true; // Нашли совпадение
                }
            }
            catch (JsonException)
            {
                // Пропускаем некорректные JSON-файлы
            }
        }

        return false; // Совпадений не найдено
    }

    public async Task<int> CountActive(Guid userId, CancellationToken ct)
    {
        var count = 0;
        var fullPath = Path.Combine(_path, userId.ToString());
        // Получаем все JSON-файлы в директории
        var jsonFiles = Directory.GetFiles(fullPath, "*.json");

        foreach (var filePath in jsonFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath, ct);
                var item = JsonSerializer.Deserialize<ToDoItem>(json);

                if (item != null &&
                    item.User.UserId == userId &&
                    item.State == ToDoItem.ToDoItemState.Active)
                {
                    count++;
                }
            }
            catch (JsonException)
            {
                // Пропускаем некорректные файлы
            }
            catch (IOException)
            {
                // Пропускаем файлы, которые не удалось прочитать
            }
        }

        return count;
    }

    public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
    {
        var result = new List<ToDoItem>();
        var fullPath = Path.Combine(_path, userId.ToString());
        // Получаем все JSON-файлы в директории
        var jsonFiles = Directory.GetFiles(fullPath, "*.json");

        foreach (var filePath in jsonFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath, ct);
                var item = JsonSerializer.Deserialize<ToDoItem>(json);

                if (item != null &&
                    item.User.UserId == userId &&
                    predicate(item))
                {
                    result.Add(item);
                }
            }
            catch (JsonException)
            {
                // Пропускаем некорректные файлы
            }
            catch (IOException)
            {
                // Пропускаем файлы с ошибками чтения
            }
        }

        return result;
    }

    private void RebuildIndex()
    {
        var index = new Dictionary<Guid, List<Guid>>(); // UserId -> List<ItemId>

        // Сканируем все поддиректории пользователей
        foreach (var userDir in Directory.GetDirectories(_path))
        {
            if (!Guid.TryParse(Path.GetFileName(userDir), out var userId))
                continue;

            // Добавляем запись для пользователя, даже если у него нет задач
            index[userId] = [];

            // Обрабатываем все файлы задач пользователя
            foreach (var filePath in Directory.GetFiles(userDir, "*.json"))
            {
                if (Guid.TryParse(Path.GetFileNameWithoutExtension(filePath), out var itemId))
                {
                    index[userId].Add(itemId);
                }
            }
        }

        // Создаем анонимный объект для сериализации
        var indexData = new { UserToItems = index };
        // Наполняем индекс
        var fullIndexPath = Path.Combine(_path, "index.json");
        var json = JsonSerializer.Serialize(indexData, SWriteOptions);
        File.WriteAllText(fullIndexPath, json);
    }

    private void UpdateIndex(Guid userId, Guid itemId, string mode)
    {
        var indexData = LoadIndex();
        if (mode == "add")
        {
            // Добавляем запись в индекс
            if (!indexData.UserToItems.TryGetValue(userId, out var value))
            {
                value = [];
                indexData.UserToItems[userId] = value;
            }

            if (!value.Contains(itemId))
            {
                value.Add(itemId);
            }
        }
        else
        {
            // Удаляем запись из индекса
            indexData.UserToItems[userId].Remove(itemId);
        }

        // Сохраняем обновленный индекс
        SaveIndex(indexData);
    }

    private IndexData LoadIndex()
    {
        var fullIndexPath = Path.Combine(_path, "index.json");
        var json = File.ReadAllText(fullIndexPath);
        var node = JsonNode.Parse(json);

        return new IndexData
        {
            UserToItems = node?["UserToItems"].Deserialize<Dictionary<Guid, List<Guid>>>() ??
                          new Dictionary<Guid, List<Guid>>(),
        };
    }

    private void SaveIndex(IndexData indexData)
    {
        var fullIndexPath = Path.Combine(_path, "index.json");
        var json = JsonSerializer.Serialize(new
        {
            indexData.UserToItems,
        }, SWriteOptions);
        File.WriteAllText(fullIndexPath, json);
    }

    private class IndexData
    {
        public Dictionary<Guid, List<Guid>> UserToItems { get; init; } = new();
    }
}