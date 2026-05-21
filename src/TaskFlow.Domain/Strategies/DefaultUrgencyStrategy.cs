using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Strategies;

public class DefaultUrgencyStrategy : IUrgencyStrategy
{
    public int CalculateUrgency(TaskItem task)
    {
        // Якщо задача виконана, її терміновість нульова
        if (task.Status == TaskFlow.Domain.Enums.TaskStatus.Done) 
            return 0;
            
        // Розрахунок на основі пріоритету
        return task.Priority switch
        {
            TaskPriority.High => 30,
            TaskPriority.Medium => 20,
            TaskPriority.Low => 10,
            _ => 0
        };
    }
}