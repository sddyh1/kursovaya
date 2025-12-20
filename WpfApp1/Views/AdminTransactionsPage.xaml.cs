using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Models;
using WpfApp1.Repositories;

namespace WpfApp1.Views
{
    public partial class AdminTransactionsPage : Page
    {
        private readonly IUserRepository _userRepository;
        private readonly ITransactionRepository _transactionRepository;
        private List<Transaction> _allTransactions = new List<Transaction>();
        private List<User> _allUsers = new List<User>();
        private bool _isInitialized = false;

        public class TransactionWithUser
        {
            public int TransactionId { get; set; }
            public string Description { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string Type { get; set; } = string.Empty;
            public string CategoryName { get; set; } = string.Empty;
            public DateTime TransactionDate { get; set; }
            public int UserId { get; set; }
            public string Username { get; set; } = string.Empty;
        }

        public AdminTransactionsPage(IUserRepository userRepository, ITransactionRepository transactionRepository)
        {
            InitializeComponent();
            _userRepository = userRepository;
            _transactionRepository = transactionRepository;

            Loaded += async (s, e) =>
            {
                await LoadUsersAsync();
                await LoadTransactionsAsync();
                _isInitialized = true;
            };
        }

        private async System.Threading.Tasks.Task LoadUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                _allUsers = users?.ToList() ?? new List<User>();

                UserFilterComboBox.Items.Clear();
                UserFilterComboBox.Items.Add(new ComboBoxItem { Content = "Все пользователи" });

                foreach (var user in _allUsers)
                {
                    UserFilterComboBox.Items.Add(user);
                }

                UserFilterComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadTransactionsAsync()
        {
            try
            {
                var transactions = await _transactionRepository.GetAllAsync();
                _allTransactions = transactions?.ToList() ?? new List<Transaction>();

                // Ожидаем инициализации элементов управления
                await System.Threading.Tasks.Task.Delay(100);
                UpdateTransactionsView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки транзакций: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTransactionsView()
        {
            if (_allTransactions == null || _allUsers == null) return;

            var userDict = _allUsers.ToDictionary(u => u.UserId, u => u.Username);

            var transactionsView = _allTransactions.Select(t => new TransactionWithUser
            {
                TransactionId = t.TransactionId,
                Description = t.Description,
                Amount = t.Amount,
                Type = t.Type,
                CategoryName = t.CategoryName,
                TransactionDate = t.TransactionDate,
                UserId = t.UserId,
                Username = userDict.ContainsKey(t.UserId) ? userDict[t.UserId] : "Неизвестно"
            }).ToList();

            // Применяем фильтр по пользователю
            if (UserFilterComboBox.SelectedItem is User selectedUser)
            {
                transactionsView = transactionsView.Where(t => t.UserId == selectedUser.UserId).ToList();
            }
            // Если выбрано "Все пользователи" (ComboBoxItem), то фильтр не применяем
            else if (UserFilterComboBox.SelectedItem is ComboBoxItem comboItem &&
                     comboItem.Content.ToString() == "Все пользователи")
            {
                // Ничего не делаем, оставляем все транзакции
            }

            // Применяем поиск (с проверкой на null)
            string searchText = string.Empty;
            if (SearchTextBox != null && !string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                searchText = SearchTextBox.Text.ToLower().Trim();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    transactionsView = transactionsView.Where(t =>
                        (t.Description != null && t.Description.ToLower().Contains(searchText)) ||
                        (t.CategoryName != null && t.CategoryName.ToLower().Contains(searchText)) ||
                        (t.Type != null && t.Type.ToLower().Contains(searchText)) ||
                        (t.Username != null && t.Username.ToLower().Contains(searchText))
                    ).ToList();
                }
            }

            TransactionsGrid.ItemsSource = transactionsView;
        }
        private async void AddTransactionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!_allUsers.Any())
            {
                MessageBox.Show("Нет пользователей для создания транзакции", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new AdminTransactionDialog(_allUsers);
            if (dialog.ShowDialog() == true)
            {
                var transaction = new Transaction
                {
                    Description = dialog.Description,
                    Amount = dialog.Amount,
                    Type = dialog.Type,
                    CategoryName = dialog.Category,
                    TransactionDate = dialog.TransactionDate,
                    UserId = dialog.SelectedUserId
                };

                try
                {
                    await _transactionRepository.AddAsync(transaction);
                    await LoadTransactionsAsync();
                    MessageBox.Show("Транзакция успешно добавлена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления транзакции: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void EditTransactionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionsGrid.SelectedItem is TransactionWithUser selectedTransaction)
            {
                var originalTransaction = _allTransactions.FirstOrDefault(t => t.TransactionId == selectedTransaction.TransactionId);
                if (originalTransaction != null)
                {
                    var dialog = new AdminTransactionDialog(_allUsers, originalTransaction);
                    if (dialog.ShowDialog() == true)
                    {
                        originalTransaction.Description = dialog.Description;
                        originalTransaction.Amount = dialog.Amount;
                        originalTransaction.Type = dialog.Type;
                        originalTransaction.CategoryName = dialog.Category;
                        originalTransaction.TransactionDate = dialog.TransactionDate;
                        originalTransaction.UserId = dialog.SelectedUserId;

                        try
                        {
                            await _transactionRepository.UpdateAsync(originalTransaction);
                            await LoadTransactionsAsync();
                            MessageBox.Show("Транзакция успешно обновлена", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка обновления транзакции: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите транзакцию для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteTransactionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionsGrid.SelectedItem is TransactionWithUser selectedTransaction)
            {
                var result = MessageBox.Show($"Удалить транзакцию '{selectedTransaction.Description}'?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _transactionRepository.DeleteAsync(selectedTransaction.TransactionId);
                        await LoadTransactionsAsync();
                        MessageBox.Show("Транзакция успешно удалена", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
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
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTransactionsView();
        }

        private void UserFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            UpdateTransactionsView();
        }
    }
}