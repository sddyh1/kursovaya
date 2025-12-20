using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Models;
using WpfApp1.Repositories;
using WpfApp1.Views;

namespace WpfApp1.Views
{
    public partial class AdminUsersPage : Page
    {
        private readonly IUserRepository _userRepository;
        private readonly ITransactionRepository _transactionRepository;
        private List<User> _allUsers = new List<User>();

        public AdminUsersPage(IUserRepository userRepository, ITransactionRepository transactionRepository)
        {
            InitializeComponent();
            _userRepository = userRepository;
            _transactionRepository = transactionRepository;

            Loaded += async (s, e) => await LoadUsersAsync();
        }

        private async System.Threading.Tasks.Task LoadUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                _allUsers = users?.ToList() ?? new List<User>();
                UsersGrid.ItemsSource = _allUsers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new UserDialog();
            if (dialog.ShowDialog() == true)
            {
                var newUser = new User
                {
                    Username = dialog.Username,
                    PasswordHash = dialog.Password,
                    Email = dialog.Email,
                    Role = dialog.Role,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                try
                {
                    await _userRepository.AddAsync(newUser);
                    await LoadUsersAsync();
                    MessageBox.Show("Пользователь успешно добавлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления пользователя: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void EditUserBtn_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is User selectedUser)
            {
                var dialog = new UserDialog(selectedUser);
                if (dialog.ShowDialog() == true)
                {
                    selectedUser.Username = dialog.Username;
                    if (!string.IsNullOrWhiteSpace(dialog.Password))
                    {
                        selectedUser.PasswordHash = dialog.Password;
                    }
                    selectedUser.Email = dialog.Email;
                    selectedUser.Role = dialog.Role;
                    selectedUser.IsActive = dialog.IsActive;

                    try
                    {
                        await _userRepository.UpdateAsync(selectedUser);
                        await LoadUsersAsync();
                        MessageBox.Show("Пользователь успешно обновлен", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка обновления пользователя: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteUserBtn_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is User selectedUser)
            {
                // Предотвращаем удаление администратора
                if (selectedUser.Role == "Admin")
                {
                    MessageBox.Show("Нельзя удалить пользователя с ролью Администратор", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show($"Удалить пользователя '{selectedUser.Username}'?\nВсе его транзакции также будут удалены.",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Получаем все транзакции пользователя и удаляем их
                        var userTransactions = await _transactionRepository.GetByUserIdAsync(selectedUser.UserId);
                        foreach (var transaction in userTransactions)
                        {
                            await _transactionRepository.DeleteAsync(transaction.TransactionId);
                        }

                        // Удаляем пользователя
                        await _userRepository.DeleteAsync(selectedUser.UserId);
                        await LoadUsersAsync();

                        MessageBox.Show("Пользователь успешно удален", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            await LoadUsersAsync();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allUsers == null) return;

            var searchText = SearchTextBox.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                UsersGrid.ItemsSource = _allUsers;
            }
            else
            {
                var filtered = _allUsers.Where(u =>
                    u.Username.ToLower().Contains(searchText) ||
                    u.Email.ToLower().Contains(searchText) ||
                    u.Role.ToLower().Contains(searchText)
                ).ToList();
                UsersGrid.ItemsSource = filtered;
            }
        }
    }
}