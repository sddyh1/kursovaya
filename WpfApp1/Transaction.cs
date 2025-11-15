using System;

namespace WpfApp1
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public string CategoryName { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}