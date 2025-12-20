using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class AdminTransactionDialog : Window
    {
        public string Description { get; private set; } = string.Empty;
        public decimal Amount { get; private set; }
        public string Type { get; private set; } = string.Empty;
        public string Category { get; private set; } = string.Empty;
        public DateTime TransactionDate { get; private set; }
        public int SelectedUserId { get; private set; }

        public AdminTransactionDialog(List<User> users)
        {
            InitializeComponent();
            UserComboBox.ItemsSource = users;
            DatePicker.SelectedDate = DateTime.Today;
            TimeTextBox.Text = DateTime.Now.ToString("HH:mm");

            if (users.Any())
                UserComboBox.SelectedIndex = 0;
        }

        public AdminTransactionDialog(List<User> users, Transaction transaction) : this(users)
        {
            Title = "Редактировать транзакцию";
            DescTextBox.Text = transaction.Description;
            AmountTextBox.Text = Math.Abs(transaction.Amount).ToString("F2");

            // Устанавливаем тип
            foreach (System.Windows.Controls.ComboBoxItem item in TypeComboBox.Items)
            {
                if (item.Content.ToString() == transaction.Type)
                {
                    TypeComboBox.SelectedItem = item;
                    break;
                }
            }

            // Устанавливаем категорию
            foreach (System.Windows.Controls.ComboBoxItem item in CategoryComboBox.Items)
            {
                if (item.Content.ToString() == transaction.CategoryName)
                {
                    CategoryComboBox.SelectedItem = item;
                    break;
                }
            }

            DatePicker.SelectedDate = transaction.TransactionDate;
            TimeTextBox.Text = transaction.TransactionDate.ToString("HH:mm");
            UserComboBox.SelectedValue = transaction.UserId;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Description = DescTextBox.Text;
            if (string.IsNullOrWhiteSpace(Description))
            {
                MessageBox.Show("Введите описание транзакции", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                DescTextBox.Focus();
                return;
            }

            if (!decimal.TryParse(AmountTextBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму (положительное число)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                AmountTextBox.Focus();
                AmountTextBox.SelectAll();
                return;
            }

            if (DatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                DatePicker.Focus();
                return;
            }

            if (!DateTime.TryParse(TimeTextBox.Text, out DateTime time))
            {
                MessageBox.Show("Введите корректное время (формат: HH:mm)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TimeTextBox.Focus();
                TimeTextBox.SelectAll();
                return;
            }

            TransactionDate = DatePicker.SelectedDate.Value.Date + time.TimeOfDay;
            Type = (TypeComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString() ?? "Доход";
            Category = (CategoryComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString() ?? "Еда";
            Amount = Type == "Расход" ? -amount : amount;
            SelectedUserId = (int)UserComboBox.SelectedValue;

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}