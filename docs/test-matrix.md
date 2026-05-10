# Test Matrix

| Use Case | Unit Test | Integration Test |
|----------|-----------|------------------|
| Створення задачі | `TaskItem_CreateValid_ShouldSetIdAndToDoStatus` | `I1_FullCycle_CreateSaveLoad...` |
| Зміна статусу | `TaskItem_ChangeStatus_DoneToToDo_ShouldThrow...` | `I5_UpdateTask_SaveAndLoad...` |
| Помилка файлу | - | `I4_LoadCorruptedJson_ShouldHandleFault` |
| Фільтрація LINQ | `Service_GetPendingTasks_ShouldExcludeDone` | - |