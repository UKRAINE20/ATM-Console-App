using ATM.Console.Services;
using Spectre.Console;

namespace ATM.Console.States
{
    public class MainMenuState : IAtmState
    {
        private readonly AtmService _service;

        public MainMenuState(AtmService service)
        {
            _service = service;
        }

        public void DisplayMenu()
        {
            AnsiConsole.Clear();
            var user = _service.GetCurrentUser();

            AnsiConsole.MarkupLine("[bold green] ГОЛОВНЕ МЕНЮ [/]");
            AnsiConsole.MarkupLine($"Користувач: [yellow]{user?.FullName}[/]");
            AnsiConsole.MarkupLine($"Баланс: [bold cyan]{user?.Balance:N2} грн[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("1. Переглянути баланс");
            AnsiConsole.MarkupLine("2. Поповнити рахунок");
            AnsiConsole.MarkupLine("3. Зняти гроші");
            AnsiConsole.MarkupLine("4. Переказ коштів");
            AnsiConsole.MarkupLine("5. Історія транзакцій");
            AnsiConsole.MarkupLine("6. Змінити PIN");
            AnsiConsole.MarkupLine("7. Вийти з акаунту");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("0. Вимкнути банкомат");
        }

        public void HandleInput(string input)
        {

        }
    }
}