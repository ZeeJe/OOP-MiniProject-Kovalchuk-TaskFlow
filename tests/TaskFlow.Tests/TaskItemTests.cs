using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.Tests;

public class TaskItemTests
{
    // Тест 1: Перевірка створення
    [Fact]
    public void Constructor_ValidData_ShouldCreateTask()
    {
        var task = new TaskItem("Test", "Desc", TaskPriority.High);
        Assert.Equal("Test", task.Title);
        Assert.Equal(TaskFlow.Domain.Enums.TaskStatus.ToDo, task.Status);
    }

    // Тест 2: Перевірка валідації в конструкторі
    [Fact]
    public void Constructor_EmptyTitle_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new TaskItem("", "Desc", TaskPriority.Low));
    }

    // Тест 3: Перевірка зміни статусу
    [Fact]
    public void ChangeStatus_ValidTransition_ShouldUpdateStatus()
    {
        var task = new TaskItem("Test", "Desc", TaskPriority.Medium);
        task.ChangeStatus(TaskFlow.Domain.Enums.TaskStatus.InProgress);
        Assert.Equal(TaskFlow.Domain.Enums.TaskStatus.InProgress, task.Status);
    }

    // Тест 4: Перевірка забороненої зміни статусу
    [Fact]
    public void ChangeStatus_DoneToToDo_ShouldThrowException()
    {
        var task = new TaskItem("Test", "Desc", TaskPriority.Medium);
        task.ChangeStatus(TaskFlow.Domain.Enums.TaskStatus.Done);
        Assert.Throws<InvalidOperationException>(() => task.ChangeStatus(TaskFlow.Domain.Enums.TaskStatus.ToDo));
    }

    // Тест 5: Перевірка роботи репозиторію
    [Fact]
    public void InMemoryRepository_ShouldSaveAndRetrieveTask()
    {
        var repo = new InMemoryTaskRepository();
        var task = new TaskItem("Repo Test", "Desc", TaskPriority.Low);
        
        repo.Add(task);
        var retrievedTask = repo.GetById(task.Id);
        
        Assert.NotNull(retrievedTask);
        Assert.Equal(task.Id, retrievedTask.Id);
    }
}