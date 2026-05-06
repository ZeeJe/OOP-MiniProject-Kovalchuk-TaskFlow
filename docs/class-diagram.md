# Доменна модель (Class Diagram)

```mermaid
classDiagram
    class BaseEntity {
        <<abstract>>
        +Guid Id
    }
    class User {
        +String Name
        +String Email
        +User(name, email)
    }
    class TaskItem {
        +String Title
        +String Description
        +TaskStatus Status
        +TaskPriority Priority
        +Guid? AssigneeId
        +TaskItem(title, description, priority)
        +ChangeStatus(newStatus)
        +AssignTo(userId)
    }
    class Project {
        +String Name
        +List~TaskItem~ Tasks
        +AddTask(task)
    }
    class ITaskRepository {
        <<interface>>
        +Add(TaskItem task)
        +GetAll() List~TaskItem~
        +GetById(Guid id) TaskItem
    }
    
    BaseEntity <|-- User
    BaseEntity <|-- TaskItem
    BaseEntity <|-- Project
    Project "1" *-- "many" TaskItem : contains
    ITaskRepository ..> TaskItem : manages