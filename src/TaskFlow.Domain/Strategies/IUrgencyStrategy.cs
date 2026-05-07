using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Strategies;

public interface IUrgencyStrategy
{
    // Повертає числову оцінку терміновості (чим більше, тим терміновіше)
    int CalculateUrgency(TaskItem task);
}