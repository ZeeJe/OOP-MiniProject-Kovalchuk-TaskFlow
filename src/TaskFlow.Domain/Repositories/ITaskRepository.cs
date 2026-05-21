using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Repositories;

public interface ITaskRepository
{
    void Add(TaskItem task);
    void Update(TaskItem task); // Додали можливість оновлення
    IEnumerable<TaskItem> GetAll();
    TaskItem? GetById(Guid id);
    
    // Асинхронні методи для I/O (вимога Lab 35)
    Task SaveToFileAsync(string filePath);
    Task LoadFromFileAsync(string filePath);
}