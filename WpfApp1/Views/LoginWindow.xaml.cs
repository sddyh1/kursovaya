using System;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Repositories;
using WpfApp1.Views;

namespace WpfApp1.Views
{
    public partial class LoginWindow : Window
    {
        private readonly IUserRepository _userRepository;

        public LoginWindow()
        {
            InitializeComponent();

            // Инициализируем базу данных при запуске приложения
            var dbContext = new Data.DatabaseContext("Data Source=finance.db;Version=3;");

            _userRepository = new UserRepository("Data Source=finance.db;Version=3;");

            // Фокус на поле имени пользователя при загрузке
            Loaded += (s, e) => UsernameTextBox.Focus();

            // Устанавливаем значения по умолчанию для тестирования
            UsernameTextBox.Text = "admin"; // Убрать в продакшене
            PasswordBox.Password = "admin123"; // Убрать в продакшене
        }

        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заполните все поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var isValid = await _userRepository.ValidateUserAsync(username, password);
                if (isValid)
                {
                    var user = await _userRepository.GetByUsernameAsync(username);

                    if (user != null)
                    {
                        // Проверяем активен ли пользователь
                        if (!user.IsActive)
                        {
                            MessageBox.Show("Ваш аккаунт деактивирован. Обратитесь к администратору.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Проверяем роль пользователя
                        if (user.Role == "Admin")
                        {
                            // Открываем панель администратора
                            var adminWindow = new AdminWindow(user);
                            adminWindow.Show();
                            this.Close();
                        }
                        else
                        {
                            // Открываем обычное окно для пользователя
                            var mainWindow = new MainWindow(user);
                            mainWindow.Show();
                            this.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Ошибка загрузки данных пользователя", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Неверное имя пользователя или пароль", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Data.SQLite.SQLiteException ex) when (ex.Message.Contains("no such table"))
            {
                MessageBox.Show("Ошибка базы данных. Перезапустите приложение.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка авторизации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Visibility == Visibility.Visible)
            {
                // Показываем пароль
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
                ShowPasswordButton.Content = "👁‍🗨";
                ShowPasswordButton.ToolTip = "Скрыть пароль";

                // Фокус на TextBox
                PasswordTextBox.Focus();
                PasswordTextBox.SelectAll();
            }
            else
            {
                // Скрываем пароль
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                ShowPasswordButton.Content = "👁";
                ShowPasswordButton.ToolTip = "Показать пароль";

                // Фокус на PasswordBox
                PasswordBox.Focus();
            }
        }

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Owner = this;
            registerWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            registerWindow.ShowDialog();
        }

        // Обновляем PasswordBox при изменении текста в TextBox
        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PasswordBox.Password = PasswordTextBox.Text;
        }

        // Обработка Enter для навигации
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
                LoginBtn_Click(sender, e);
            }
        }

        private void PasswordTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                LoginBtn_Click(sender, e);
            }
        }
    }
}