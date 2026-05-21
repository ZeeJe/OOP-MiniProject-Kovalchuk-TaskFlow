using System.Text.Json.Serialization;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class TaskItem : BaseEntity
{
    [JsonInclude] 
    public string Title { get; private set; }
    
    [JsonInclude] 
    public string Description { get; private set; }
    
    [JsonInclude] 
    public TaskFlow.Domain.Enums.TaskStatus Status { get; private set; }
    
    [JsonInclude] 
    public TaskPriority Priority { get; private set; }
    
    [JsonInclude] 
    public Guid? AssigneeId { get; private set; }

    // Основний конструктор для створення нових задач
    public TaskItem(string title, string description, TaskPriority priority)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Назва задачі не може бути порожньою.");

        Id = Guid.NewGuid(); // Генеруємо новий ID
        Title = title;
        Description = description;
        Priority = priority;
        Status = Enums.TaskStatus.ToDo; // За замовчуванням завжди ToDo
    }

    // Конструктор для JSON десеріалізації (щоб відновлювати з файлу)
    [JsonConstructor]
    public TaskItem(Guid id, string title, string description, TaskFlow.Domain.Enums.TaskStatus status, TaskPriority priority, Guid? assigneeId)
    {
        Id = id; // Відновлюємо старий ID з файлу
        Title = title;
        Description = description;
        Status = status;
        Priority = priority;
        AssigneeId = assigneeId;
    }

    // Метод для зміни статусу (бізнес-логіка)
    public void ChangeStatus(Enums.TaskStatus newStatus)
    {
        // Перевірка інваріанту: не можна повернути Done у ToDo
        if (Status == Enums.TaskStatus.Done && newStatus == Enums.TaskStatus.ToDo)
            throw new InvalidOperationException("Неможливо повернути завершену задачу назад у ToDo.");
        
        Status = newStatus;
    }
}