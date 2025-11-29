using System;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class TransactionDialog : Window
    {
        public string Description { get; private set; } = string.Empty;
        public decimal Amount { get; private set; }
        public string Type { get; private set; } = string.Empty;
        public string Category { get; private set; } = string.Empty;
        public DateTime TransactionDate { get; private set; }

        public TransactionDialog()
        {
            InitializeComponent();
            DatePicker.SelectedDate = DateTime.Today;
            TimeTextBox.Text = DateTime.Now.ToString("HH:mm");
        }

        public TransactionDialog(Transaction transaction) : this()
        {
            Title = "Редактировать транзакцию";
            DescTextBox.Text = transaction.Description;
            AmountTextBox.Text = Math.Abs(transaction.Amount).ToString("F2");

            // Устанавливаем тип
            foreach (ComboBoxItem item in TypeComboBox.Items)
            {
                if (item.Content.ToString() == transaction.Type)
                {
                    TypeComboBox.SelectedItem = item;
                    break;
                }
            }

            // Устанавливаем категорию
            foreach (ComboBoxItem item in CategoryComboBox.Items)
            {
                if (item.Content.ToString() == transaction.CategoryName)
                {
                    CategoryComboBox.SelectedItem = item;
                    break;
                }
            }

            DatePicker.SelectedDate = transaction.TransactionDate;
            TimeTextBox.Text = transaction.TransactionDate.ToString("HH:mm");
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
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

            // Проверяем дату
            if (DatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                DatePicker.Focus();
                return;
            }

            // Проверяем время
            if (!DateTime.TryParse(TimeTextBox.Text, out DateTime time))
            {
                MessageBox.Show("Введите корректное время (формат: HH:mm)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TimeTextBox.Focus();
                TimeTextBox.SelectAll();
                return;
            }

            // Объединяем дату и время
            TransactionDate = DatePicker.SelectedDate.Value.Date + time.TimeOfDay;

            Type = (TypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Income";
            Category = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Food";
            Amount = Type == "Expense" ? -amount : amount;

            DialogResult = true;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}