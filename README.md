# Банкомат (ATM Console Application)

## Опис функціональності

Консольна програма імітує роботу банкомату з такими можливостями:

- Реєстрація нової картки
- Авторизація за номером картки та PIN-кодом 
- Перегляд балансу
- Поповнення рахунку
- Зняття готівки
- Переказ коштів іншому користувачу
- Перегляд історії транзакцій
- Зміна PIN 
- Збереження даних у JSON-файлах (`Data/users.json`, `Data/transactions.json`)

## Як запустити програму

1. Відкрити рішення в Visual Studio
2. Вибрати проєкт `ATM.Console` як стартовий
3. Натиснути `F5`

## Використані технології
- C# + .NET 8
- Spectre.Console (красивий UI)
- System.Text.Json (збереження даних)

## Programming Principles (5+)

- **Single Responsibility Principle** — кожен клас відповідає за свою зону (Repository — тільки робота з файлами, Service — бізнес-логіка).
- **Open/Closed Principle** — легко додавати нові операції через State Pattern.
- **Dependency Inversion** — високорівневі модулі не залежать від низькорівневих.
- **Separation of Concerns** — розділення UI, бізнес-логіки та даних.
- **DRY (Don't Repeat Yourself)** — методи збереження/завантаження даних винесені в репозиторій.
- **KISS** — код зроблено максимально зрозумілим.

## Design Patterns (3+)

1. **State Pattern** — `IAtmState`, `IdleState`, `MainMenuState` — керування станами банкомата (`States/`).
2. **Repository Pattern** — `JsonRepository` для роботи з файлами.
3. **Service Layer** — `AtmService` як шар бізнес-логіки.

## Refactoring Techniques

- Extract Method
- Extract Class
- Rename variables/methods для кращої читабельності
- Introduce Parameter Object (частково)
- Replace Magic Numbers with constants
- Move Method

## Структура проєкту

- `Models/` — сутності (User, Transaction)
- `Repositories/` — робота з даними
- `Services/` — бізнес-логіка
- `States/` — State Pattern
- `Program.cs` — головний цикл програми

