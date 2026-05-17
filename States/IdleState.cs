using ATM.Console.Services;
using Spectre.Console;

namespace ATM.Console.States
{
    public class IdleState : IAtmState
    {
        private readonly AtmService _service;

        public IdleState(AtmService service)
        {
            _service = service;
        }

        public void DisplayMenu()
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold yellow] БАНКОМАТ [/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("1. Вхід у систему");
            AnsiConsole.MarkupLine("2. Реєстрація нової картки");
            AnsiConsole.MarkupLine("0. Вихід");
            AnsiConsole.WriteLine();
        }

        public void HandleInput(string input)
        {
        }
    }
}