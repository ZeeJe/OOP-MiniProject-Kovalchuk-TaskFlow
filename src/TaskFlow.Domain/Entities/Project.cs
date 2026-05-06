namespace TaskFlow.Domain.Entities;

public class Project : BaseEntity
{
    public string Name { get; private set; }
    private readonly List<TaskItem> _tasks = new();
    
    // Інкапсульована колекція (тільки для читання ззовні)
    public IReadOnlyCollection<TaskItem> Tasks => _tasks.AsReadOnly();

    public Project(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Назва проєкту не може бути порожньою.");
        Name = name;
    }

    public void AddTask(TaskItem task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        _tasks.Add(task);
    }
}