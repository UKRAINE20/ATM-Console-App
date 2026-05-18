using ATM.Console.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ATM.Console.Repositories
{
    public class JsonRepository : IAtmRepository
    {
        private readonly string _usersPath = "Data/users.json";
        private readonly string _transactionsPath = "Data/transactions.json";

        public JsonRepository()
        {
            Directory.CreateDirectory("Data");

            if (!File.Exists(_usersPath))
                File.WriteAllText(_usersPath, "[]");

            if (!File.Exists(_transactionsPath))
                File.WriteAllText(_transactionsPath, "[]");
        }

        //  Users 
        public List<User> LoadUsers()
        {
            var json = File.ReadAllText(_usersPath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        public void SaveUsers(List<User> users)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_usersPath, JsonSerializer.Serialize(users, options));
        }

        //  Transactions 
        public List<Transaction> LoadTransactions()
        {
            var json = File.ReadAllText(_transactionsPath);
            return JsonSerializer.Deserialize<List<Transaction>>(json) ?? new List<Transaction>();
        }

        public void SaveTransactions(List<Transaction> transactions)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_transactionsPath, JsonSerializer.Serialize(transactions, options));
        }
    }
}
