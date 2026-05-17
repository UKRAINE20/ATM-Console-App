using ATM.Console.Models;
using ATM.Console.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace ATM.Console.Services
{
    public class AtmService
    {
        private readonly JsonRepository _repository;
        private User _currentUser;

        public AtmService()
        {
            _repository = new JsonRepository();
        }

        public bool Login(string cardNumber, string pin)
        {
            var users = _repository.LoadUsers();
            var user = users.FirstOrDefault(u => u.CardNumber == cardNumber);

            if (user == null || user.IsBlocked)
                return false;

            if (user.Pin == pin)
            {
                user.FailedAttempts = 0;
                _currentUser = user;
                _repository.SaveUsers(users);
                return true;
            }
            else
            {
                user.FailedAttempts++;
                if (user.FailedAttempts >= 3)
                    user.IsBlocked = true;

                _repository.SaveUsers(users);
                return false;
            }
        }

        public User GetCurrentUser() => _currentUser;

        public void Logout()
        {
            _currentUser = null;
        }

        public decimal GetBalance()
        {
            return _currentUser?.Balance ?? 0;
        }

        public bool Deposit(decimal amount)
        {
            if (_currentUser == null || amount <= 0) return false;

            var users = _repository.LoadUsers();
            var user = users.FirstOrDefault(u => u.CardNumber == _currentUser.CardNumber);

            if (user != null)
            {
                user.Balance += amount;
                _currentUser.Balance = user.Balance;

                SaveTransaction("Deposit", amount, user.Balance);
                _repository.SaveUsers(users);
                return true;
            }
            return false;
        }

        public bool Withdraw(decimal amount)
        {
            if (_currentUser == null || amount <= 0 || _currentUser.Balance < amount)
                return false;

            var users = _repository.LoadUsers();
            var user = users.FirstOrDefault(u => u.CardNumber == _currentUser.CardNumber);

            if (user != null)
            {
                user.Balance -= amount;
                _currentUser.Balance = user.Balance;

                SaveTransaction("Withdraw", amount, user.Balance);
                _repository.SaveUsers(users);
                return true;
            }
            return false;
        }

        public bool Transfer(string targetCard, decimal amount)
        {
            if (_currentUser == null || amount <= 0 || _currentUser.Balance < amount)
                return false;

            var users = _repository.LoadUsers();
            var sender = users.FirstOrDefault(u => u.CardNumber == _currentUser.CardNumber);
            var receiver = users.FirstOrDefault(u => u.CardNumber == targetCard);

            if (sender != null && receiver != null)
            {
                sender.Balance -= amount;
                receiver.Balance += amount;
                _currentUser.Balance = sender.Balance;

                SaveTransaction("Transfer", amount, sender.Balance, targetCard);
                _repository.SaveUsers(users);
                return true;
            }
            return false;
        }

        private void SaveTransaction(string type, decimal amount, decimal balanceAfter, string targetCard = null)
        {
            var transactions = _repository.LoadTransactions();
            var transaction = new Transaction
            {
                CardNumber = _currentUser?.CardNumber ?? "",
                Type = type,
                Amount = amount,
                BalanceAfter = balanceAfter,
                TargetCard = targetCard
            };

            transactions.Add(transaction);
            _repository.SaveTransactions(transactions);
        }

        public List<Transaction> GetTransactionHistory()
        {
            var all = _repository.LoadTransactions();
            return all.Where(t => t.CardNumber == _currentUser?.CardNumber)
                      .OrderByDescending(t => t.Date)
                      .ToList();
        }

        public List<User> GetAllUsers() => _repository.LoadUsers();

        // Реєстрація нового користувача
        public bool RegisterUser(string cardNumber, string pin, string fullName, decimal initialBalance)
        {
            var users = _repository.LoadUsers();

            if (users.Any(u => u.CardNumber == cardNumber))
            {
                return false;
            }

            var newUser = new User
            {
                CardNumber = cardNumber,
                Pin = pin,
                FullName = fullName,
                Balance = initialBalance,
                IsBlocked = false,
                FailedAttempts = 0
            };

            users.Add(newUser);
            _repository.SaveUsers(users);
            return true;
        }

        // Зміна PIN-коду
        public bool ChangePin(string oldPin, string newPin)
        {
            if (_currentUser == null)
                return false;

            // Перевірка старого PIN
            if (_currentUser.Pin != oldPin)
                return false;

            // Перевірка нового PIN (має бути 4 цифри)
            if (string.IsNullOrWhiteSpace(newPin) || newPin.Length != 4 || !newPin.All(char.IsDigit))
                return false;

            var users = _repository.LoadUsers();
            var user = users.FirstOrDefault(u => u.CardNumber == _currentUser.CardNumber);

            if (user != null)
            {
                user.Pin = newPin;
                _currentUser.Pin = newPin;

                SaveTransaction("PinChange", 0, user.Balance);
                _repository.SaveUsers(users);
                return true;
            }

            return false;
        }
    }
}