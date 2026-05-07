using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Strategies;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.Application.Services;

namespace TaskFlow.Tests;

public class TaskFlowTests
{
    // --- ТЕСТИ ДОМЕННОЇ МОДЕЛІ (Інваріанти) ---

    [Fact]
    public void TaskItem_CreateValid_ShouldSetToDoStatus()
    {
        var task = new TaskItem("Test", "Desc", TaskPriority.High);
        Assert.Equal(TaskFlow.Domain.Enums.TaskStatus.ToDo, task.Status);
    }

    [Fact]
    public void TaskItem_EmptyTitle_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => new TaskItem("", "Desc", TaskPriority.Low));
    }

    [Fact]
    public void TaskItem_ChangeStatus_DoneToToDo_ShouldThrow()
    {
        var task = new TaskItem("Test", "Desc", TaskPriority.Low);
        task.ChangeStatus(TaskFlow.Domain.Enums.TaskStatus.Done);
        Assert.Throws<InvalidOperationException>(() => task.ChangeStatus(TaskFlow.Domain.Enums.TaskStatus.ToDo));
    }

    [Fact]
    public void Project_AddTask_ShouldIncreaseCount()
    {
        var project = new Project("Lab 35");
        project.AddTask(new TaskItem("Task 1", "Desc", TaskPriority.High));
        Assert.Single(project.Tasks);
    }

    // --- ТЕСТИ ПАТЕРНУ STRATEGY ---

    [Fact]
    public void Strategy_DoneTask_ShouldReturnZeroUrgency()
    {
        var strategy = new DefaultUrgencyStrategy();
        var task = new TaskItem("Test", "Desc", TaskPriority.High);
        task.ChangeStatus(TaskFlow.Domain.Enums.TaskStatus.Done);
        
        var urgency = strategy.CalculateUrgency(task);
        Assert.Equal(0, urgency);
    }

    [Fact]
    public void Strategy_HighPriority_ShouldReturn30()
    {
        var strategy = new DefaultUrgencyStrategy();
        var task = new TaskItem("Test", "Desc", TaskPriority.High);
        
        var urgency = strategy.CalculateUrgency(task);
        Assert.Equal(30, urgency);
    }

    // --- ТЕСТИ INFRASTRUCTURE (Persistence) ---

    [Fact]
    public void JsonRepository_AddAndGetById_ShouldReturnSameTask()
    {
        var repo = new JsonTaskRepository();
        var task = new TaskItem("Repo Test", "Desc", TaskPriority.Medium);
        
        repo.Add(task);
        var retrieved = repo.GetById(task.Id);
        
        Assert.NotNull(retrieved);
        Assert.Equal(task.Title, retrieved.Title);
    }

    [Fact]
    public void JsonRepository_UpdateTask_ShouldModifyExisting()
    {
        var repo = new JsonTaskRepository();
        var task = new TaskItem("Old", "Desc", TaskPriority.Low);
        repo.Add(task);

        task.ChangeStatus(TaskFlow.Domain.Enums.TaskStatus.InProgress);
        repo.Update(task);

        var updated = repo.GetById(task.Id);
        Assert.Equal(TaskFlow.Domain.Enums.TaskStatus.InProgress, updated!.Status);
    }

    // --- ТЕСТИ БІЗНЕС-ЛОГІКИ ТА LINQ (Application) ---

    [Fact]
    public void Service_GetPendingTasks_ShouldFilterOutDone()
    {
        var repo = new InMemoryTaskRepository();
        var service = new TaskService(repo);
        
        var id1 = service.CreateTask("Task 1", "D", TaskPriority.Low);
        var id2 = service.CreateTask("Task 2", "D", TaskPriority.Low);
        service.ChangeTaskStatus(id1, TaskFlow.Domain.Enums.TaskStatus.Done);

        var pending = service.GetPendingTasks().ToList();
        
        Assert.Single(pending);
        Assert.Equal(id2, pending[0].Id);
    }

    [Fact]
    public void Service_GetTasksByPriority_ShouldFilterProperly()
    {
        var repo = new InMemoryTaskRepository();
        var service = new TaskService(repo);
        
        service.CreateTask("Low", "D", TaskPriority.Low);
        service.CreateTask("High", "D", TaskPriority.High);

        var highTasks = service.GetTasksByPriority(TaskPriority.High).ToList();
        
        Assert.Single(highTasks);
        Assert.Equal("High", highTasks[0].Title);
    }

    [Fact]
    public void Service_GetStatusStatistics_ShouldGroupCorrectly()
    {
        var repo = new InMemoryTaskRepository();
        var service = new TaskService(repo);
        
        service.CreateTask("T1", "D", TaskPriority.Low); // ToDo
        service.CreateTask("T2", "D", TaskPriority.Low); // ToDo

        var stats = service.GetStatusStatistics();
        
        Assert.True(stats.ContainsKey(TaskFlow.Domain.Enums.TaskStatus.ToDo));
        Assert.Equal(2, stats[TaskFlow.Domain.Enums.TaskStatus.ToDo]);
    }

    [Fact]
    public void Service_GetMostUrgentTask_ShouldReturnHighest()
    {
        var repo = new InMemoryTaskRepository();
        var service = new TaskService(repo);
        var strategy = new DefaultUrgencyStrategy();
        
        service.CreateTask("Low Priority", "D", TaskPriority.Low);
        service.CreateTask("High Priority", "D", TaskPriority.High); // Це найтерміновіша

        var urgent = service.GetMostUrgentTask(strategy);
        
        Assert.NotNull(urgent);
        Assert.Equal(TaskPriority.High, urgent.Priority);
    }
}