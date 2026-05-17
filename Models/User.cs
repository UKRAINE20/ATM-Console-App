using System;

namespace ATM.Console.Models
{
    public class User
    {
        public string CardNumber { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public decimal Balance { get; set; } = 0;
        public bool IsBlocked { get; set; } = false;
        public int FailedAttempts { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}