using ATM.Console.Services;
using ATM.Console.States;
using ATM.Console.Repositories; // Додали юзінг для репозиторію
using Spectre.Console;
using System;
using System.Linq;
using System.Text;

namespace ATM.Console
{
    public class Program
    {
        private static AtmService _service;
        private static IAtmState _currentState;

        static void Main(string[] args)
        {
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;
            System.Console.InputEncoding = System.Text.Encoding.UTF8;
            
            var repository = new JsonRepository();
            _service = new AtmService(repository);

            _currentState = new IdleState(_service);

            while (true)
            {
                _currentState.DisplayMenu();
                string input = AnsiConsole.Ask<string>("[bold]Ваш вибір:[/] ");

                try
                {
                    HandleInput(input);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Помилка: {ex.Message}[/]");
                    System.Console.ReadKey();
                }
            }
        }

        private static void HandleInput(string input)
        {
            if (_currentState is IdleState)
            {
                HandleIdleState(input);
            }
            else if (_currentState is MainMenuState)
            {
                HandleMainMenuState(input);
            }
        }

        private static void HandleIdleState(string input)
        {
            if (input == "1") // Вхід
            {
                string card = AnsiConsole.Ask<string>("Номер картки: ");
                string pin = AnsiConsole.Ask<string>("PIN-код: ");

                if (_service.Login(card, pin))
                {
                    _currentState = new MainMenuState(_service);
                    AnsiConsole.MarkupLine("[green]Вхід успішний![/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Невірний номер картки або PIN![/]");
                }
                System.Console.ReadKey();
            }
            else if (input == "2") // Реєстрація
            {
                RegisterNewUser();
            }
            else if (input == "0")
            {
                AnsiConsole.MarkupLine("[yellow]До побачення![/]");
                Environment.Exit(0);
            }
        }

        private static void HandleMainMenuState(string input)
        {
            switch (input)
            {
                case "1": // Баланс
                    AnsiConsole.MarkupLine($"[bold cyan]Ваш баланс: {_service.GetBalance():N2} грн[/]");
                    break;

                case "2": // Поповнення
                    decimal deposit = AnsiConsole.Ask<decimal>("Сума поповнення: ");
                    if (_service.Deposit(deposit))
                        AnsiConsole.MarkupLine("[green]Рахунок поповнено![/]");
                    else
                        AnsiConsole.MarkupLine("[red]Помилка поповнення![/]");
                    break;

                case "3": // Зняття
                    decimal withdraw = AnsiConsole.Ask<decimal>("Сума для зняття: ");
                    if (_service.Withdraw(withdraw))
                        AnsiConsole.MarkupLine("[green]Гроші видані![/]");
                    else
                        AnsiConsole.MarkupLine("[red]Недостатньо коштів![/]");
                    break;

                case "4": // Переказ
                    string target = AnsiConsole.Ask<string>("Номер картки отримувача: ");
                    decimal amount = AnsiConsole.Ask<decimal>("Сума переказу: ");
                    if (_service.Transfer(target, amount))
                        AnsiConsole.MarkupLine("[green]Переказ успішний![/]");
                    else
                        AnsiConsole.MarkupLine("[red]Помилка переказу![/]");
                    break;

                case "5": // Історія
                    ShowHistory();
                    break;

                case "6": // Зміна PIN
                    ChangePinCode();
                    break;

                case "7": // Вихід з акаунту
                    _service.Logout();
                    _currentState = new IdleState(_service);
                    AnsiConsole.MarkupLine("[yellow]Ви вийшли з акаунту.[/]");
                    break;

                case "0": // Вимкнення банкомату
                    AnsiConsole.MarkupLine("[yellow]До побачення![/]");
                    Environment.Exit(0);
                    break;

                default:
                    AnsiConsole.MarkupLine("[red]Невірний вибір![/]");
                    break;
            }

            System.Console.ReadKey();
        }

        private static void RegisterNewUser()
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold yellow]=== Реєстрація нової картки ===[/]");

            string cardNumber = AnsiConsole.Ask<string>("Номер картки: ");
            string pin = AnsiConsole.Ask<string>("PIN-код (4 цифри): ");
            string name = AnsiConsole.Ask<string>("Прізвище та ім'я: ");
            decimal balance = AnsiConsole.Ask<decimal>("Початковий баланс (грн): ");

            if (_service.RegisterUser(cardNumber, pin, name, balance))
            {
                AnsiConsole.MarkupLine("[green]Картка успішно зареєстрована![/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Картка з таким номером вже існує![/]");
            }

            System.Console.ReadKey();
        }

        private static void ShowHistory()
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold] Історія транзакцій [/]");
            AnsiConsole.WriteLine();

            var history = _service.GetTransactionHistory();

            if (history.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Історія транзакцій порожня.[/]");
            }
            else
            {
                foreach (var t in history.Take(10))
                {
                    AnsiConsole.WriteLine($"[{t.Date:dd.MM.yyyy HH:mm}] {t.Type,-12} | {t.Amount,10:N2} грн | Баланс: {t.BalanceAfter:N2}");
                }
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[gray]Натисніть будь-яку клавішу для повернення...[/]");
            System.Console.ReadKey();
        }

        private static void ChangePinCode()
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold yellow] Зміна PIN-коду [/]");

            string oldPin = AnsiConsole.Ask<string>("Введіть поточний PIN: ");
            string newPin = AnsiConsole.Ask<string>("Введіть новий PIN (4 цифри): ");
            string confirmPin = AnsiConsole.Ask<string>("Підтвердіть новий PIN: ");

            if (newPin != confirmPin)
            {
                AnsiConsole.MarkupLine("[red]PIN-коди не співпадають![/]");
                System.Console.ReadKey();
                return;
            }

            if (_service.ChangePin(oldPin, newPin))
            {
                AnsiConsole.MarkupLine("[green]PIN-код успішно змінено![/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Невірний поточний PIN або помилка![/]");
            }

            System.Console.ReadKey();
        }
    }
}