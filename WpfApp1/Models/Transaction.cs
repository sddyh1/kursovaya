using System;
using System.ComponentModel;

namespace WpfApp1.Models
{
    public class Transaction : INotifyPropertyChanged
    {
        private int _transactionId;
        private string _description = string.Empty;
        private decimal _amount;
        private string _type = string.Empty;
        private string _categoryName = string.Empty;
        private DateTime _transactionDate;
        private int _userId;

        public int TransactionId
        {
            get => _transactionId;
            set { _transactionId = value; OnPropertyChanged(nameof(TransactionId)); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        public decimal Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(nameof(Amount)); }
        }

        public string Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        public string CategoryName
        {
            get => _categoryName;
            set { _categoryName = value; OnPropertyChanged(nameof(CategoryName)); }
        }

        public DateTime TransactionDate
        {
            get => _transactionDate;
            set { _transactionDate = value; OnPropertyChanged(nameof(TransactionDate)); }
        }

        public int UserId
        {
            get => _userId;
            set { _userId = value; OnPropertyChanged(nameof(UserId)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}