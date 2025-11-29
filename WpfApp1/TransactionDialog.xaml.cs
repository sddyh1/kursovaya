using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class TransactionDialog : Window
    {
        public string Description => DescTextBox.Text;
        public decimal Amount { get; private set; }
        public string Type => (TypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
        public string Category => (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
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

            foreach (ComboBoxItem item in TypeComboBox.Items)
            {
                if (item.Content.ToString() == transaction.Type)
                {
                    TypeComboBox.SelectedItem = item;
                    break;
                }
            }

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

            Amount = Type == "Expense" ? -amount : amount;
            DialogResult = true;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}