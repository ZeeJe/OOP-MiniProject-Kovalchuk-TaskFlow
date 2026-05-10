# Тестова документація (Quality Gate)

## Запуск тестів
Для запуску всіх тестів із перевіркою покриття (Coverage) використовуйте команду:
`dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover`

## Покриті сценарії
- **Unit Tests (20 шт):** Перевірка інваріантів доменних сутностей, поведінка патерну Strategy, бізнес-правила в TaskService (за допомогою Moq).
- **Integration Tests (8 шт):** Повний I/O цикл (збереження та відновлення з JSON), реакція на пошкоджені файли (Fault Handling).