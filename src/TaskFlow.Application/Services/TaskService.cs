using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Repositories;
using TaskFlow.Domain.Strategies;

namespace TaskFlow.Application.Services;

public class TaskService
{
    private readonly ITaskRepository _repository;

    public TaskService(ITaskRepository repository) 
    {
        _repository = repository;
    }

    // --- Базові операції ---

    public Guid CreateTask(string title, string description, TaskPriority priority)
    {
        var task = new TaskItem(title, description, priority);
        _repository.Add(task);
        return task.Id;
    }

    public IEnumerable<TaskItem> GetAllTasks() => _repository.GetAll();

    public void ChangeTaskStatus(Guid taskId, TaskFlow.Domain.Enums.TaskStatus newStatus)
    {
        var task = _repository.GetById(taskId);
        if (task == null) throw new TaskFlow.Domain.Exceptions.TaskNotFoundException(taskId); // Використовуємо наш виняток
        
        task.ChangeStatus(newStatus);
        _repository.Update(task);
    }

    // --- Робота з файлами (Асинхронна) ---

    public async Task SaveDataAsync(string filePath)
    {
        await _repository.SaveToFileAsync(filePath);
    }

    public async Task LoadDataAsync(string filePath)
    {
        await _repository.LoadFromFileAsync(filePath);
    }

    // --- LINQ Запити та Аналітика (Вимога Lab 35) ---

    // 1. Фільтрація: Отримати всі невиконані задачі
    public IEnumerable<TaskItem> GetPendingTasks()
    {
        return _repository.GetAll().Where(t => t.Status != TaskFlow.Domain.Enums.TaskStatus.Done);
    }

    // 2. Сортування та Фільтрація: Задачі за пріоритетом
    public IEnumerable<TaskItem> GetTasksByPriority(TaskPriority priority)
    {
        return _repository.GetAll()
            .Where(t => t.Priority == priority)
            .OrderBy(t => t.Title);
    }

    // 3. Агрегація/Групування: Статистика за статусами
    public Dictionary<TaskFlow.Domain.Enums.TaskStatus, int> GetStatusStatistics()
    {
        return _repository.GetAll()
            .GroupBy(t => t.Status)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // 4. Застосування патерну Strategy + LINQ: Пошук найтерміновішої задачі
    public TaskItem? GetMostUrgentTask(IUrgencyStrategy strategy)
    {
        return _repository.GetAll()
            .Where(t => t.Status != TaskFlow.Domain.Enums.TaskStatus.Done)
            .OrderByDescending(t => strategy.CalculateUrgency(t))
            .FirstOrDefault();
    }
}