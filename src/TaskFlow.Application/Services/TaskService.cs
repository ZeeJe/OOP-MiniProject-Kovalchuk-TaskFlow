using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Repositories;

namespace TaskFlow.Application.Services;

public class TaskService
{
    private readonly ITaskRepository _repository;

    // Впровадження залежностей (Dependency Injection) через конструктор
    public TaskService(ITaskRepository repository) 
    {
        _repository = repository;
    }

    public Guid CreateTask(string title, string description, TaskPriority priority)
    {
        // Створюємо нову задачу за правилами домену
        var task = new TaskItem(title, description, priority);
        
        // Зберігаємо її через репозиторій
        _repository.Add(task);
        
        return task.Id;
    }

    public IEnumerable<TaskItem> GetAllTasks()
    {
        return _repository.GetAll();
    }

    public void ChangeTaskStatus(Guid taskId, TaskFlow.Domain.Enums.TaskStatus newStatus)
    {
        var task = _repository.GetById(taskId);
        if (task == null)
        {
            throw new Exception("Задачу не знайдено.");
        }
            
        task.ChangeStatus(newStatus);
    }
}