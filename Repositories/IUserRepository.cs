using System.Collections.Generic;
using ATM.Console.Models;

namespace ATM.Console.Repositories
{
    public interface IUserRepository
    {
        List<User> LoadUsers();
        void SaveUsers(List<User> users);
        List<Transaction> LoadTransactions();
        void SaveTransactions(List<Transaction> transactions);
    }
}
