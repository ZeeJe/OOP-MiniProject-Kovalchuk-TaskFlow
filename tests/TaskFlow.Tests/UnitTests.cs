using Moq;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Domain.Repositories;
using TaskFlow.Domain.Strategies;
using TaskFlow.Application.Services;

namespace TaskFlow.Tests;

public class UnitTests
{
    // --- ТЕСТИ СУТНОСТЕЙ (ІНВАРІАНТИ ТА ПРИКОРДОННІ ЗНАЧЕННЯ) ---
    
    [Fact] public void TaskItem_CreateValid_ShouldSetIdAndToDoStatus() {
        var task = new TaskItem("Title", "Desc", TaskPriority.High);
        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal(TaskFlow.Domain.Enums.TaskStatus.ToDo, task.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void TaskItem_InvalidTitle_ShouldThrowArgumentException(string? invalidTitle) {
        Assert.Throws<ArgumentException>(() => new TaskItem(invalidTitle!, "Desc", TaskPriority.Low));
    }

    [Fact] public void TaskItem_ChangeStatus_Valid_ShouldUpdate() {
        var task = new TaskItem("Title", "Desc", TaskPriority.Medium);
        task.ChangeStatus(TaskFlow.Domain.Enums.TaskStatus.InProgress);
        Assert.Equal(TaskFlow.Domain.Enums.TaskStatus.InProgress, task.Status);
    }

    [Fact] public void TaskItem_ChangeStatus_DoneToToDo_ShouldThrowInvalidOperation() {
        var task = new TaskItem("Title", "Desc", TaskPriority.Medium);
        task.ChangeStatus(TaskFlow.Domain.Enums.TaskStatus.Done);
        Assert.Throws<InvalidOperationException>(() => task.ChangeStatus(TaskFlow.Domain.Enums.TaskStatus.ToDo));
    }

    [Fact] public void User_CreateValid_ShouldSetProperties() {
        var user = new User("John", "john@mail.com");
        Assert.Equal("John", user.Name);
    }

    [Fact] public void User_EmptyName_ShouldThrow() {
        Assert.Throws<ArgumentException>(() => new User("", "mail"));
    }

    [Fact] public void Project_CreateValid_ShouldSetProperties() {
        var proj = new Project("New Project");
        Assert.Empty(proj.Tasks);
    }

    [Fact] public void Project_AddNullTask_ShouldThrowArgumentNull() {
        var proj = new Project("Proj");
        Assert.Throws<ArgumentNullException>(() => proj.AddTask(null!));
    }

    [Fact] public void Project_AddTask_ShouldIncreaseCount() {
        var proj = new Project("Proj");
        proj.AddTask(new TaskItem("T", "D", TaskPriority.Low));
        Assert.Single(proj.Tasks);
    }

    // --- ТЕСТИ ПАТЕРНУ STRATEGY ---
    
    [Fact] public void Strategy_DoneTask_ReturnsZero() {
        var task = new TaskItem("T", "D", TaskPriority.High);
        task.ChangeStatus(TaskFlow.Domain.Enums.TaskStatus.Done);
        Assert.Equal(0, new DefaultUrgencyStrategy().CalculateUrgency(task));
    }

    [Theory]
    [InlineData(TaskPriority.High, 30)]
    [InlineData(TaskPriority.Medium, 20)]
    [InlineData(TaskPriority.Low, 10)]
    public void Strategy_CalculatesCorrectly(TaskPriority priority, int expectedScore) {
        var task = new TaskItem("T", "D", priority);
        Assert.Equal(expectedScore, new DefaultUrgencyStrategy().CalculateUrgency(task));
    }

    // --- ТЕСТИ СЕРВІСУ (З ВИКОРИСТАННЯМ MOQ) ---
    
    [Fact] public void Service_CreateTask_ShouldCallRepositoryAdd() {
        var mockRepo = new Mock<ITaskRepository>();
        var service = new TaskService(mockRepo.Object);
        service.CreateTask("Title", "Desc", TaskPriority.Medium);
        mockRepo.Verify(r => r.Add(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact] public void Service_ChangeStatus_NotFound_ShouldThrowDomainException() {
        var mockRepo = new Mock<ITaskRepository>();
        mockRepo.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((TaskItem?)null);
        var service = new TaskService(mockRepo.Object);
        Assert.Throws<TaskNotFoundException>(() => service.ChangeTaskStatus(Guid.NewGuid(), TaskFlow.Domain.Enums.TaskStatus.Done));
    }

    [Fact] public void Service_ChangeStatus_Found_ShouldCallUpdate() {
        var task = new TaskItem("T", "D", TaskPriority.Low);
        var mockRepo = new Mock<ITaskRepository>();
        mockRepo.Setup(r => r.GetById(task.Id)).Returns(task);
        var service = new TaskService(mockRepo.Object);
        
        service.ChangeTaskStatus(task.Id, TaskFlow.Domain.Enums.TaskStatus.InProgress);
        mockRepo.Verify(r => r.Update(task), Times.Once);
        Assert.Equal(TaskFlow.Domain.Enums.TaskStatus.InProgress, task.Status);
    }

    [Fact] public void Service_GetAllTasks_ShouldReturnRepoData() {
        var mockRepo = new Mock<ITaskRepository>();
        mockRepo.Setup(r => r.GetAll()).Returns(new List<TaskItem> { new TaskItem("T", "D", TaskPriority.Low) });
        var service = new TaskService(mockRepo.Object);
        Assert.Single(service.GetAllTasks());
    }

    [Fact] public void Service_GetPendingTasks_ShouldExcludeDone() {
        var t1 = new TaskItem("T1", "D", TaskPriority.Low);
        var t2 = new TaskItem("T2", "D", TaskPriority.Low);
        t2.ChangeStatus(TaskFlow.Domain.Enums.TaskStatus.Done);
        
        var mockRepo = new Mock<ITaskRepository>();
        mockRepo.Setup(r => r.GetAll()).Returns(new List<TaskItem> { t1, t2 });
        var service = new TaskService(mockRepo.Object);
        
        var pending = service.GetPendingTasks().ToList();
        Assert.Single(pending);
        Assert.Equal("T1", pending[0].Title);
    }

    [Fact] public void Service_GetTasksByPriority_ShouldSortAndFilter() {
        var t1 = new TaskItem("B", "D", TaskPriority.High);
        var t2 = new TaskItem("A", "D", TaskPriority.High);
        var mockRepo = new Mock<ITaskRepository>();
        mockRepo.Setup(r => r.GetAll()).Returns(new List<TaskItem> { t1, t2 });
        var service = new TaskService(mockRepo.Object);
        
        var list = service.GetTasksByPriority(TaskPriority.High).ToList();
        Assert.Equal("A", list[0].Title); // Перевірка сортування
    }

    [Fact] public void Service_GetStatusStatistics_ShouldGroup() {
        var mockRepo = new Mock<ITaskRepository>();
        var t1 = new TaskItem("T", "D", TaskPriority.Low);
        mockRepo.Setup(r => r.GetAll()).Returns(new List<TaskItem> { t1, t1 });
        var service = new TaskService(mockRepo.Object);
        
        var stats = service.GetStatusStatistics();
        Assert.Equal(2, stats[TaskFlow.Domain.Enums.TaskStatus.ToDo]);
    }
}