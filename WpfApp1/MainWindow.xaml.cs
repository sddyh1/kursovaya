using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Windows;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Transaction> transactions;
        private string connectionString = "Data Source=finance.db;Version=3;";

        public MainWindow()
        {
            transactions = new ObservableCollection<Transaction>();
            InitializeComponent();
            InitializeDatabase();
            LoadTransactions();
        }

        private void InitializeDatabase()
        {
            try
            {
                if (!File.Exists("finance.db"))
                {
                    SQLiteConnection.CreateFile("finance.db");

                    using (var connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        var command = new SQLiteCommand(@"
                            CREATE TABLE IF NOT EXISTS Transactions (
                                TransactionId INTEGER PRIMARY KEY AUTOINCREMENT,
                                Description TEXT NOT NULL,
                                Amount DECIMAL(10,2) NOT NULL,
                                Type TEXT NOT NULL,
                                CategoryName TEXT NOT NULL,
                                TransactionDate DATETIME NOT NULL
                            )", connection);
                        command.ExecuteNonQuery();

                        command.CommandText = @"
                            INSERT INTO Transactions (Description, Amount, Type, CategoryName, TransactionDate) VALUES
                            ('Зарплата', 50000.00, 'Income', 'Salary', datetime('now', '-7 days')),
                            ('Продукты', -3500.50, 'Expense', 'Food', datetime('now', '-6 days')),
                            ('Коммунальные услуги', -2500.00, 'Expense', 'Utilities', datetime('now', '-5 days')),
                            ('Фриланс', 15000.00, 'Income', 'Freelance', datetime('now', '-4 days'))";
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации базы данных: {ex.Message}");
            }

        private void LoadTransactions()
        {
            try
            {
                transactions.Clear();

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    var command = new SQLiteCommand(
                        "SELECT * FROM Transactions ORDER BY TransactionDate DESC",
                        connection);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transactions.Add(new Transaction
                            {
                                TransactionId = reader.GetInt32(0),
                                Description = reader.GetString(1),
                                Amount = reader.GetDecimal(2),
                                Type = reader.GetString(3),
                                CategoryName = reader.GetString(4),
                                TransactionDate = reader.GetDateTime(5)
                            });
                        }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void UpdateStats()
        {
            try
            {
                decimal income = 0, expense = 0;

                foreach (var transaction in transactions)
                {
                    if (transaction.Type == "Income")
                        income += transaction.Amount;
                    else
                        expense += transaction.Amount;
                }

                var balance = income + expense;

                StatsText.Text = $"Доходы: {income:N2} ₽ | Расходы: {expense:N2} ₽ | Баланс: {balance:N2} ₽";
            }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TransactionDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        var command = new SQLiteCommand(@"
                            INSERT INTO Transactions (Description, Amount, Type, CategoryName, TransactionDate)
                            VALUES (@desc, @amount, @type, @category, @date)", connection);

                        command.Parameters.AddWithValue("@desc", dialog.Description);
                        command.Parameters.AddWithValue("@amount", dialog.Amount);
                        command.Parameters.AddWithValue("@type", dialog.Type);
                        command.Parameters.AddWithValue("@category", dialog.Category);
                        command.Parameters.AddWithValue("@date", dialog.TransactionDate);

                        command.ExecuteNonQuery();
                    }
                    LoadTransactions();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления: {ex.Message}");
                }
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionsGrid.SelectedItem is Transaction selected)
            {
                var dialog = new TransactionDialog(selected);
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        using (var connection = new SQLiteConnection(connectionString))
                        {
                            connection.Open();
                            var command = new SQLiteCommand(@"
                                UPDATE Transactions 
                                SET Description = @desc, Amount = @amount, Type = @type, 
                                    CategoryName = @category, TransactionDate = @date
                                WHERE TransactionId = @id", connection);

                            command.Parameters.AddWithValue("@desc", dialog.Description);
                            command.Parameters.AddWithValue("@amount", dialog.Amount);
                            command.Parameters.AddWithValue("@type", dialog.Type);
                            command.Parameters.AddWithValue("@category", dialog.Category);
                            command.Parameters.AddWithValue("@date", dialog.TransactionDate);
                            command.Parameters.AddWithValue("@id", selected.TransactionId);

                            command.ExecuteNonQuery();
                        }
                        LoadTransactions();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка редактирования: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите транзакцию для редактирования");
            }
        }

        private async void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionsGrid.SelectedItem is Transaction selected)
            {
                var result = MessageBox.Show(
                    $"Удалить транзакцию '{selected.Description}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var connection = new SQLiteConnection(connectionString))
                        {
                            connection.Open();
                            var command = new SQLiteCommand(
                                "DELETE FROM Transactions WHERE TransactionId = @id",
                                connection);
                            command.Parameters.AddWithValue("@id", selected.TransactionId);
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                MessageBox.Show("Выберите транзакцию для удаления", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
                    }

        private async void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            await LoadTransactionsAsync();
            MessageBox.Show("Данные обновлены", "Информация",
                          MessageBoxButton.OK, MessageBoxImage.Information);
                }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allTransactions == null) return;

            var searchText = SearchTextBox.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                TransactionsGrid.ItemsSource = _allTransactions;
            }
            else
            {
                var filtered = _allTransactions.Where(t =>
                    t.Description.ToLower().Contains(searchText) ||
                    t.CategoryName.ToLower().Contains(searchText) ||
                    t.Type.ToLower().Contains(searchText)
                ).ToList();
                TransactionsGrid.ItemsSource = filtered;
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода",
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_transactionRepository != null)
        {
                _transactionRepository.DataChanged -= async (s, ev) => await LoadTransactionsAsync();
            }
            base.OnClosed(e);
        }
    }
}