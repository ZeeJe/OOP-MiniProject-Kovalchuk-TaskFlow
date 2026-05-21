using System.Text.Json;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Repositories;

namespace TaskFlow.Infrastructure.Repositories;

public class JsonTaskRepository : ITaskRepository
{
    // Спеціалізована колекція (вимога Lab 35). Dictionary забезпечує швидкий пошук O(1) за ID.
    private Dictionary<Guid, TaskItem> _tasks = new();

    public void Add(TaskItem task)
    {
        if (_tasks.ContainsKey(task.Id))
            throw new InvalidOperationException("Задача з таким ID вже існує.");
            
        _tasks[task.Id] = task;
    }

    public void Update(TaskItem task)
    {
        if (!_tasks.ContainsKey(task.Id))
            throw new KeyNotFoundException("Задачу не знайдено для оновлення.");
            
        _tasks[task.Id] = task;
    }

    public IEnumerable<TaskItem> GetAll() => _tasks.Values.ToList();

    public TaskItem? GetById(Guid id) => _tasks.TryGetValue(id, out var task) ? task : null;

    // Асинхронне збереження (вимога Lab 35: File I/O, Async)
    public async Task SaveToFileAsync(string filePath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        
        // Використовуємо using для контролю ресурсів
        using FileStream createStream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(createStream, _tasks.Values, options);
    }

    // Асинхронне завантаження з обробкою помилок (вимога Lab 35: контрольовані помилки I/O)
    public async Task LoadFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Файл даних не знайдено. Буде створено нову базу.");
            return;
        }

        try
        {
            using FileStream openStream = File.OpenRead(filePath);
            var loadedTasks = await JsonSerializer.DeserializeAsync<List<TaskItem>>(openStream);
            
            if (loadedTasks != null)
            {
                _tasks = loadedTasks.ToDictionary(t => t.Id, t => t);
            }
        }
        catch (JsonException)
        {
            Console.WriteLine("Помилка: Файл пошкоджено. Дані не завантажено.");
            _tasks = new Dictionary<Guid, TaskItem>();
        }
    }
}