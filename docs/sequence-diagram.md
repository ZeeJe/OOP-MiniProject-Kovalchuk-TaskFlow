# Сценарій: Створення нової задачі
```mermaid
sequenceDiagram
    actor User
    participant UI as Console Menu
    participant App as TaskService
    participant Domain as TaskItem
    participant Repo as ITaskRepository

    User->>UI: Вводить назву, опис, пріоритет
    UI->>App: CreateTask(title, desc, priority)
    App->>Domain: new TaskItem(title, desc, priority)
    Domain-->>App: task instance
    App->>Repo: Add(task)
    Repo-->>App: success
    App-->>UI: Task created (ID)
    UI-->>User: "Задачу успішно створено!"