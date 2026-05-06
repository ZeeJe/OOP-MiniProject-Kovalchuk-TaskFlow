using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Repositories;

public interface ITaskRepository
{
    void Add(TaskItem task);
    IEnumerable<TaskItem> GetAll();
    TaskItem? GetById(Guid id);
}