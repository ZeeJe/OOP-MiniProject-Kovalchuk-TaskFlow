using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

// Наслідуємося від BaseEntity, щоб автоматично отримати Id
public class TaskItem : BaseEntity
{
    // Властивості закриті для зміни ззовні (private set)
    public string Title { get; private set; }
    public string Description { get; private set; }
    public TaskFlow.Domain.Enums.TaskStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public Guid? AssigneeId { get; private set; }

    // Конструктор, який не дозволить створити задачу без назви
    public TaskItem(string title, string description, TaskPriority priority)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Назва задачі не може бути порожньою.");

        Title = title;
        Description = description;
        Priority = priority;
        Status = Enums.TaskStatus.ToDo; // За замовчуванням завжди ToDo
    }

    // Метод для зміни статусу (інкапсульована бізнес-логіка)
    public void ChangeStatus(Enums.TaskStatus newStatus)
    {
        // Перевірка інваріанту: не можна повернути Done у ToDo
        if (Status == Enums.TaskStatus.Done && newStatus == Enums.TaskStatus.ToDo)
            throw new InvalidOperationException("Неможливо повернути завершену задачу назад у ToDo.");
        
        Status = newStatus;
    }
}