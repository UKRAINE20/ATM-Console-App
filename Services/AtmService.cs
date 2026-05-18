using ATM.Console.Models;
using ATM.Console.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATM.Console.Services
{
    public class AtmService
    {
        private const int MaxFailedAttempts = 3;
        private const int PinLength = 4;

        private readonly JsonRepository _repository;

        private User _currentUser;

        public AtmService()
        {
            _repository = new JsonRepository();
        }

        public bool Login(string cardNumber, string pin)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) ||
                string.IsNullOrWhiteSpace(pin))
            {
                return false;
            }

            var users = _repository.LoadUsers();

            var user = FindUser(users, cardNumber);

            if (user == null || user.IsBlocked)
            {
                return false;
            }

            if (user.Pin == pin)
            {
                user.FailedAttempts = 0;

                _currentUser = user;

                _repository.SaveUsers(users);

                return true;
            }

            user.FailedAttempts++;

            if (user.FailedAttempts >= MaxFailedAttempts)
            {
                user.IsBlocked = true;
            }

            _repository.SaveUsers(users);

            return false;
        }

        public User GetCurrentUser()
        {
            return _currentUser;
        }

        public void Logout()
        {
            _currentUser = null;
        }

        public decimal GetBalance()
        {
            if (_currentUser == null)
            {
                return 0;
            }

            return _currentUser.Balance;
        }

        public bool Deposit(decimal amount)
        {
            if (_currentUser == null || amount <= 0)
            {
                return false;
            }

            var users = _repository.LoadUsers();

            var user = FindCurrentUser(users);

            if (user == null)
            {
                return false;
            }

            user.Balance += amount;

            _currentUser.Balance = user.Balance;

            SaveTransaction("Deposit", amount, user.Balance);

            _repository.SaveUsers(users);

            return true;
        }

        public bool Withdraw(decimal amount)
        {
            if (_currentUser == null ||
                amount <= 0 ||
                _currentUser.Balance < amount)
            {
                return false;
            }

            var users = _repository.LoadUsers();

            var user = FindCurrentUser(users);

            if (user == null)
            {
                return false;
            }

            user.Balance -= amount;

            _currentUser.Balance = user.Balance;

            SaveTransaction("Withdraw", amount, user.Balance);

            _repository.SaveUsers(users);

            return true;
        }

        public bool Transfer(string targetCard, decimal amount)
        {
            if (_currentUser == null ||
                amount <= 0 ||
                _currentUser.Balance < amount)
            {
                return false;
            }

            var users = _repository.LoadUsers();

            var sender = FindCurrentUser(users);

            var receiver = FindUser(users, targetCard);

            if (sender == null || receiver == null)
            {
                return false;
            }

            if (sender.CardNumber == receiver.CardNumber)
            {
                return false;
            }

            sender.Balance -= amount;

            receiver.Balance += amount;

            _currentUser.Balance = sender.Balance;

            SaveTransaction(
                "Transfer",
                amount,
                sender.Balance,
                receiver.CardNumber);

            _repository.SaveUsers(users);

            return true;
        }

        public List<Transaction> GetTransactionHistory()
        {
            if (_currentUser == null)
            {
                return new List<Transaction>();
            }

            return _repository
                .LoadTransactions()
                .Where(t => t.CardNumber == _currentUser.CardNumber)
                .OrderByDescending(t => t.Date)
                .ToList();
        }

        public List<User> GetAllUsers()
        {
            return _repository.LoadUsers();
        }

        public bool RegisterUser(
            string cardNumber,
            string pin,
            string fullName,
            decimal initialBalance)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) ||
                string.IsNullOrWhiteSpace(pin) ||
                string.IsNullOrWhiteSpace(fullName) ||
                initialBalance < 0)
            {
                return false;
            }

            if (!IsPinValid(pin))
            {
                return false;
            }

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

        public bool ChangePin(string oldPin, string newPin)
        {
            if (_currentUser == null)
            {
                return false;
            }

            if (_currentUser.Pin != oldPin)
            {
                return false;
            }

            if (!IsPinValid(newPin))
            {
                return false;
            }

            var users = _repository.LoadUsers();

            var user = FindCurrentUser(users);

            if (user == null)
            {
                return false;
            }

            user.Pin = newPin;

            _currentUser.Pin = newPin;

            SaveTransaction("PinChange", 0, user.Balance);

            _repository.SaveUsers(users);

            return true;
        }

        private static bool IsPinValid(string pin)
        {
            return !string.IsNullOrWhiteSpace(pin) &&
                   pin.Length == PinLength &&
                   pin.All(char.IsDigit);
        }

        private User FindCurrentUser(List<User> users)
        {
            return users.FirstOrDefault(
                u => u.CardNumber == _currentUser.CardNumber);
        }

        private User FindUser(List<User> users, string cardNumber)
        {
            return users.FirstOrDefault(
                u => u.CardNumber == cardNumber);
        }

        private void SaveTransaction(
            string type,
            decimal amount,
            decimal balanceAfter,
            string targetCard = null)
        {
            if (_currentUser == null)
            {
                return;
            }

            var transactions = _repository.LoadTransactions();

            var transaction = new Transaction
            {
                CardNumber = _currentUser.CardNumber,
                Type = type,
                Amount = amount,
                BalanceAfter = balanceAfter,
                TargetCard = targetCard
            };

            transactions.Add(transaction);

            _repository.SaveTransactions(transactions);
        }
    }
}
