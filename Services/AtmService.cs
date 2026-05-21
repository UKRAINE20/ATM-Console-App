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
            List<User> users = _repository.LoadUsers();
            User user = users.FirstOrDefault(u => u.CardNumber == cardNumber);

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

            _currentUser.Balance += amount;

            UpdateUserInList();
            SaveTransaction("Deposit", amount, _currentUser.Balance);

            return true;
        }

        public bool Withdraw(decimal amount)
        {
            if (_currentUser == null || amount <= 0 || _currentUser.Balance < amount)
                return false;

            _currentUser.Balance -= amount;

            UpdateUserInList();
            SaveTransaction("Withdraw", amount, _currentUser.Balance);

            return true;
        }

        public bool Transfer(string targetCard, decimal amount)
        {
            if (_currentUser == null || amount <= 0 || _currentUser.Balance < amount)
                return false;

            List<User> users = _repository.LoadUsers();
            User sender = users.FirstOrDefault(u => u.CardNumber == _currentUser.CardNumber);
            User receiver = users.FirstOrDefault(u => u.CardNumber == targetCard);

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
            List<Transaction> transactions = _repository.LoadTransactions();
            Transaction transaction = new Transaction
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
            List<Transaction> all = _repository.LoadTransactions();
            return all.Where(t => t.CardNumber == _currentUser?.CardNumber)
                      .OrderByDescending(t => t.Date)
                      .ToList();
        }

        public List<User> GetAllUsers() => _repository.LoadUsers();

        public bool RegisterUser(string cardNumber, string pin, string fullName, decimal initialBalance)
        {
            List<User> users = _repository.LoadUsers();

            if (users.Any(u => u.CardNumber == cardNumber))
            {
                return false;
            }

            User newUser = new User
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
            if (_currentUser == null || _currentUser.Pin != oldPin)
                return false;

            if (string.IsNullOrWhiteSpace(newPin) || newPin.Length != 4 || !newPin.All(char.IsDigit))
                return false;

            _currentUser.Pin = newPin;

            UpdateUserInList();
            SaveTransaction("PinChange", 0, _currentUser.Balance);

            return true;
        }

        private void UpdateUserInList()
        {
            List<User> users = _repository.LoadUsers();
            User userInDb = users.FirstOrDefault(u => u.CardNumber == _currentUser.CardNumber);

            if (userInDb != null)
            {
                userInDb.Balance = _currentUser.Balance;
                userInDb.Pin = _currentUser.Pin;
                userInDb.FailedAttempts = _currentUser.FailedAttempts;
                userInDb.IsBlocked = _currentUser.IsBlocked;

                _repository.SaveUsers(users);
            }
        }
    }
}