using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Repositories;

namespace TaskFlow.Infrastructure.Repositories;

public class InMemoryTaskRepository : ITaskRepository
{
    private readonly List<TaskItem> _tasks = new();

    public void Add(TaskItem task)
    {
        _tasks.Add(task);
    }

    // Новий метод оновлення
    public void Update(TaskItem task)
    {
        var existingTask = _tasks.FirstOrDefault(t => t.Id == task.Id);
        if (existingTask != null)
        {
            _tasks.Remove(existingTask);
            _tasks.Add(task);
        }
    }
    
    public IEnumerable<TaskItem> GetAll()
    {
        return _tasks.ToList();
    }
    
    public TaskItem? GetById(Guid id)
    {
        return _tasks.FirstOrDefault(t => t.Id == id);
    }

    // Заглушка для in-memory сховища, оскільки воно не працює з реальними файлами
    public Task SaveToFileAsync(string filePath)
    {
        return Task.CompletedTask;
    }

    // Заглушка для in-memory сховища
    public Task LoadFromFileAsync(string filePath)
    {
        return Task.CompletedTask;
    }
}