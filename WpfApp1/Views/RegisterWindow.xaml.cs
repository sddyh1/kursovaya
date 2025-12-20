using System;
using System.Windows;
using System.Windows.Controls;
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
            Loaded += (s, e) => UsernameTextBox.Focus();
        }

        private void ShowPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Visibility == Visibility.Visible)
            {
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
                ShowPasswordButton.Content = "👁‍🗨";
                ShowPasswordButton.ToolTip = "Скрыть пароль";
                PasswordTextBox.Focus();
                PasswordTextBox.SelectAll();
            }
            else
            {
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                ShowPasswordButton.Content = "👁";
                ShowPasswordButton.ToolTip = "Показать пароль";
                PasswordBox.Focus();
            }
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

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PasswordBox.Password = PasswordTextBox.Text;
        }

        private void UsernameTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (PasswordBox.Visibility == Visibility.Visible)
                    PasswordBox.Focus();
                else
                    PasswordTextBox.Focus();
            }
        }

        private void PasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                EmailTextBox.Focus();
            }
        }

        private void PasswordTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                EmailTextBox.Focus();
            }
        }

        private void EmailTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                RegisterBtn_Click(sender, e);
            }
        }
    }
}