using System;
using System.Windows;
using WpfApp1.Models;
using WpfApp1.Repositories;

namespace WpfApp1.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly IUserRepository _userRepository;

        public RegisterWindow()
        {
            InitializeComponent();
            _userRepository = new UserRepository("Data Source=finance.db;Version=3;");
        }

        private async void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;
            var email = EmailTextBox.Text;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Заполните все поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (await _userRepository.UsernameExistsAsync(username))
                {
                    MessageBox.Show("Пользователь с таким именем уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newUser = new User
                {
                    Username = username,
                    PasswordHash = password,
                    Email = email,
                    Role = "User",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                await _userRepository.AddAsync(newUser);
                MessageBox.Show("Регистрация успешна!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (System.Data.SQLite.SQLiteException ex) when (ex.Message.Contains("no such table"))
            {
                MessageBox.Show("Ошибка базы данных. Перезапустите приложение.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}