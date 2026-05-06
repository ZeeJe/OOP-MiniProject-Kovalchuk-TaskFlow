using TaskFlow.Application.Services;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.ConsoleApp;

class Program
{
    static void Main(string[] args)
    {
        var repository = new InMemoryTaskRepository();
        var taskService = new TaskService(repository);

        while (true)
        {
            Console.WriteLine("\n--- TaskFlow Menu ---");
            Console.WriteLine("1. Створити задачу");
            Console.WriteLine("2. Показати всі задачі");
            Console.WriteLine("3. Змінити статус задачі");
            Console.WriteLine("4. Вихід");
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
                    priority = TaskPriority.Medium; // За замовчуванням
                
                try
                {
                    var id = taskService.CreateTask(title, desc, priority);
                    Console.WriteLine($"Задачу успішно створено! ID: {id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка: {ex.Message}");
                }
            }
            else if (input == "2")
            {
                var tasks = taskService.GetAllTasks();
                Console.WriteLine("\n--- Список задач ---");
                foreach (var task in tasks)
                {
                    Console.WriteLine($"[{task.Status}] {task.Title} (Пріоритет: {task.Priority}) | ID: {task.Id}");
                }
            }
            else if (input == "3")
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
                            Console.WriteLine("Статус успішно змінено!");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Помилка: {ex.Message}");
                        }
                    }
                    else Console.WriteLine("Невірний статус.");
                }
                else Console.WriteLine("Невірний формат ID.");
            }
            else if (input == "4") break;
        }
    }
}