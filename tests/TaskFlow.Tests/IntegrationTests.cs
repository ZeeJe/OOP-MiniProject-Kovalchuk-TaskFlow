using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.Application.Services;

namespace TaskFlow.Tests;

public class IntegrationTests : IDisposable
{
    private readonly string _tempFile;

    public IntegrationTests()
    {
        // Для кожного тесту створюємо унікальний тимчасовий файл
        _tempFile = Path.GetTempFileName();
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile)) File.Delete(_tempFile);
    }

    [Fact] public async Task I1_FullCycle_CreateSaveLoad_ShouldRestoreState() {
        var repo1 = new JsonTaskRepository();
        var service1 = new TaskService(repo1);
        service1.CreateTask("Int Test", "Desc", TaskPriority.High);
        await service1.SaveDataAsync(_tempFile);

        var repo2 = new JsonTaskRepository();
        var service2 = new TaskService(repo2);
        await service2.LoadDataAsync(_tempFile);
        
        var tasks = service2.GetAllTasks().ToList();
        Assert.Single(tasks);
        Assert.Equal("Int Test", tasks[0].Title);
    }

    [Fact] public async Task I2_SaveEmpty_ShouldCreateValidJson() {
        var repo = new JsonTaskRepository();
        await repo.SaveToFileAsync(_tempFile);
        var content = await File.ReadAllTextAsync(_tempFile);
        Assert.Contains("[", content);
        Assert.Contains("]", content);
    }

    [Fact] public async Task I3_LoadFromNonExistentFile_ShouldNotCrash() {
        var repo = new JsonTaskRepository();
        await repo.LoadFromFileAsync("fake_path.json");
        Assert.Empty(repo.GetAll()); // Має просто створити порожню колекцію
    }

    [Fact] public async Task I4_LoadCorruptedJson_ShouldHandleFault() {
        await File.WriteAllTextAsync(_tempFile, "{ broken json ]");
        var repo = new JsonTaskRepository();
        await repo.LoadFromFileAsync(_tempFile);
        Assert.Empty(repo.GetAll()); // Fault handling: повертає порожню базу, а не крашить програму
    }

    [Fact] public async Task I5_UpdateTask_SaveAndLoad_ShouldPreserveChanges() {
        var repo1 = new JsonTaskRepository();
        var task = new TaskItem("T", "D", TaskPriority.Low);
        repo1.Add(task);
        task.ChangeStatus(TaskFlow.Domain.Enums.TaskStatus.InProgress);
        repo1.Update(task);
        await repo1.SaveToFileAsync(_tempFile);

        var repo2 = new JsonTaskRepository();
        await repo2.LoadFromFileAsync(_tempFile);
        var loadedTask = repo2.GetById(task.Id);
        Assert.Equal(TaskFlow.Domain.Enums.TaskStatus.InProgress, loadedTask!.Status);
    }

    [Fact] public void I6_Repository_AddDuplicate_ShouldThrow() {
        var repo = new JsonTaskRepository();
        var task = new TaskItem("T", "D", TaskPriority.Low);
        repo.Add(task);
        Assert.Throws<InvalidOperationException>(() => repo.Add(task));
    }

    [Fact] public void I7_Repository_UpdateNonExistent_ShouldThrow() {
        var repo = new JsonTaskRepository();
        var task = new TaskItem("T", "D", TaskPriority.Low);
        Assert.Throws<KeyNotFoundException>(() => repo.Update(task));
    }

    [Fact] public async Task I8_MultipleSaves_ShouldOverwriteCorrectly() {
        var repo = new JsonTaskRepository();
        var t1 = new TaskItem("1", "1", TaskPriority.Low);
        repo.Add(t1);
        await repo.SaveToFileAsync(_tempFile);
        
        var t2 = new TaskItem("2", "2", TaskPriority.Low);
        repo.Add(t2);
        await repo.SaveToFileAsync(_tempFile); // Друге збереження

        var repo2 = new JsonTaskRepository();
        await repo2.LoadFromFileAsync(_tempFile);
        Assert.Equal(2, repo2.GetAll().Count());
    }
}