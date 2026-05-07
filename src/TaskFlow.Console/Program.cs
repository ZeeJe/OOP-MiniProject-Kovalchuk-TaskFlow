using TaskFlow.Application.Services;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.Domain.Strategies;

namespace TaskFlow.ConsoleApp;

class Program
{
    // Змінили на async Task, бо читання/запис файлів асинхронні
    static async Task Main(string[] args)
    {
        // Підключаємо JSON репозиторій замість In-Memory
        var repository = new JsonTaskRepository();
        var taskService = new TaskService(repository);
        var urgencyStrategy = new DefaultUrgencyStrategy(); // Наш патерн
        
        string dbPath = "tasks.json";

        // 1. Відновлюємо дані при старті програми
        await taskService.LoadDataAsync(dbPath);

        while (true)
        {
            Console.WriteLine("\n=== TaskFlow Menu (Ітерація 2) ===");
            Console.WriteLine("1. Створити задачу");
            Console.WriteLine("2. Змінити статус задачі");
            Console.WriteLine("3. Показати всі задачі");
            Console.WriteLine("4. Показати невиконані задачі (LINQ Фільтрація)");
            Console.WriteLine("5. Статистика за статусами (LINQ Групування)");
            Console.WriteLine("6. Знайти найтерміновішу задачу (Патерн Strategy)");
            Console.WriteLine("7. Зберегти та Вийти");
            Console.Write("Оберіть опцію: ");

            var input = Console.ReadLine();

            if (input == "1")
            {
                Console.Write("Назва: ");
                string title = Console.ReadLine() ?? "";
                Console.Write("Опис: ");
                string desc = Console.ReadLine() ?? "";
                Console.Write("Пріоритет (0 - Low, 1 - Medium, 2 - High): ");
                if (!Enum.TryParse(Console.ReadLine(), out TaskPriority priority))
                    priority = TaskPriority.Medium;

                try
                {
                    var id = taskService.CreateTask(title, desc, priority);
                    await taskService.SaveDataAsync(dbPath); // Автозбереження після змін
                    Console.WriteLine($"Задачу успішно створено! ID: {id}");
                }
                catch (Exception ex) { Console.WriteLine($"Помилка: {ex.Message}"); }
            }
            else if (input == "2")
            {
                Console.Write("Введіть ID задачі: ");
                if (Guid.TryParse(Console.ReadLine(), out Guid taskId))
                {
                    Console.Write("Новий статус (0 - ToDo, 1 - InProgress, 2 - Done): ");
                    if (Enum.TryParse(Console.ReadLine(), out TaskFlow.Domain.Enums.TaskStatus newStatus))
                    {
                        try
                        {
                            taskService.ChangeTaskStatus(taskId, newStatus);
                            await taskService.SaveDataAsync(dbPath); // Автозбереження
                            Console.WriteLine("Статус успішно змінено!");
                        }
                        catch (Exception ex) { Console.WriteLine($"Помилка: {ex.Message}"); }
                    }
                    else Console.WriteLine("Невірний статус.");
                }
                else Console.WriteLine("Невірний формат ID.");
            }
            else if (input == "3")
            {
                var tasks = taskService.GetAllTasks();
                Console.WriteLine("\n--- Всі задачі ---");
                foreach (var t in tasks) 
                    Console.WriteLine($"[{t.Status}] {t.Title} (Пріоритет: {t.Priority}) | ID: {t.Id}");
            }
            else if (input == "4")
            {
                var tasks = taskService.GetPendingTasks();
                Console.WriteLine("\n--- Невиконані задачі ---");
                foreach (var t in tasks) 
                    Console.WriteLine($"[{t.Status}] {t.Title}");
            }
            else if (input == "5")
            {
                var stats = taskService.GetStatusStatistics();
                Console.WriteLine("\n--- Статистика ---");
                foreach (var stat in stats) 
                    Console.WriteLine($"{stat.Key}: {stat.Value} задач");
            }
            else if (input == "6")
            {
                var urgentTask = taskService.GetMostUrgentTask(urgencyStrategy);
                if (urgentTask != null)
                    Console.WriteLine($"\nНайтерміновіша: [{urgentTask.Status}] {urgentTask.Title} (Пріоритет: {urgentTask.Priority})");
                else
                    Console.WriteLine("\nНемає активних задач.");
            }
            else if (input == "7")
            {
                await taskService.SaveDataAsync(dbPath);
                Console.WriteLine("Дані збережено. Вихід...");
                break;
            }
        }
    }
}