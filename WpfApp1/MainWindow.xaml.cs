using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp1.Models;
using WpfApp1.Repositories;
using WpfApp1.Views;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly User _currentUser;
        private List<Transaction> _allTransactions = new List<Transaction>();

        public MainWindow(User user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }

            InitializeComponent();
            _currentUser = user;
            _transactionRepository = new TransactionRepository("Data Source=finance.db;Version=3;");

            // Подписываемся на событие изменения данных
            _transactionRepository.DataChanged += async (s, e) => await LoadTransactionsAsync();

            InitializeUI();


            // Добавляем обработчик для снятия выделения при повторном клике
            TransactionsGrid.PreviewMouseDown += TransactionsGrid_PreviewMouseDown;

            // Используем discard для асинхронного вызова
            _ = LoadTransactionsAsync();
        }

        private void InitializeUI()
        {
            CurrentUserText.Text = _currentUser.Username;
            Title = $"Finance Manager - {_currentUser.Username}";
        }

        private async System.Threading.Tasks.Task LoadTransactionsAsync()
        {
            try
            {
                var transactions = await _transactionRepository.GetByUserIdAsync(_currentUser.UserId);
                _allTransactions = transactions?.ToList() ?? new List<Transaction>();
                TransactionsGrid.ItemsSource = _allTransactions;
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics()
        {
            if (_allTransactions == null || !_allTransactions.Any())
            {
                StatsText.Text = "Баланс: 0,00 ₽";
                IncomeText.Text = "Доходы: 0,00 ₽";
                ExpenseText.Text = "Расходы: 0,00 ₽";
                return;
            }

            var totalIncome = _allTransactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            var totalExpenses = _allTransactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            var balance = totalIncome + totalExpenses;

            StatsText.Text = $"Баланс: {balance:N2} ₽";
            IncomeText.Text = $"Доходы: {totalIncome:N2} ₽";
            ExpenseText.Text = $"Расходы: {Math.Abs(totalExpenses):N2} ₽";
        }

        // Обработчик для снятия выделения при повторном клике на строку
        private void TransactionsGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            // Получаем элемент под курсором
            var hitTestResult = VisualTreeHelper.HitTest(dataGrid, e.GetPosition(dataGrid));

            if (hitTestResult != null)
            {
                var row = FindParent<DataGridRow>(hitTestResult.VisualHit);
                if (row != null)
                {
                    // Если строка уже выделена, снимаем выделение
                    if (row.IsSelected)
                    {
                        row.IsSelected = false;
                        e.Handled = true; // Предотвращаем дальнейшую обработку
                    }
                }
            }
        }

        // Вспомогательный метод для поиска родительского элемента
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            if (parentObject is T parent)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        private async void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TransactionDialog();
            if (dialog.ShowDialog() == true)
            {
                var transaction = new Transaction
                {
                    Description = dialog.Description,
                    Amount = dialog.Amount,
                    Type = dialog.Type,
                    CategoryName = dialog.Category,
                    TransactionDate = dialog.TransactionDate,
                    UserId = _currentUser.UserId
                };

                try
                {
                    await _transactionRepository.AddAsync(transaction);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления транзакции: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        private async void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionsGrid.SelectedItem is Transaction selectedTransaction)
            {
                var dialog = new TransactionDialog(selectedTransaction);
                if (dialog.ShowDialog() == true)
                {
                    selectedTransaction.Description = dialog.Description;
                    selectedTransaction.Amount = dialog.Amount;
                    selectedTransaction.Type = dialog.Type;
                    selectedTransaction.CategoryName = dialog.Category;
                    selectedTransaction.TransactionDate = dialog.TransactionDate;

                    try
                    {
                        await _transactionRepository.UpdateAsync(selectedTransaction);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка редактирования транзакции: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите транзакцию для редактирования", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionsGrid.SelectedItem is Transaction selectedTransaction)
            {
                var result = MessageBox.Show($"Удалить транзакцию '{selectedTransaction.Description}'?",
                                           "Подтверждение удаления",
                                           MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _transactionRepository.DeleteAsync(selectedTransaction.TransactionId);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления транзакции: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
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
                // Открываем окно входа
                var loginWindow = new LoginWindow();
                loginWindow.Show();

                // Закрываем текущее окно
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Отписываемся от событий при закрытии окна
            if (_transactionRepository != null)
            {
                _transactionRepository.DataChanged -= async (s, ev) => await LoadTransactionsAsync();
            }

            if (TransactionsGrid != null)
            {
                TransactionsGrid.PreviewMouseDown -= TransactionsGrid_PreviewMouseDown;
            }

            base.OnClosed(e);
        }
    }
}