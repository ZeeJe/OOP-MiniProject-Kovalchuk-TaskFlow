namespace TaskFlow.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class TaskNotFoundException : DomainException
{
    public TaskNotFoundException(Guid taskId) : base($"Задачу з ID {taskId} не знайдено.") { }
}