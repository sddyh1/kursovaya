using System.Windows;
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
            _userRepository = new UserRepository("Data Source=finance.db;Version=3;");
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

                    var mainWindow = new MainWindow(user);
                    mainWindow.Show();
                    this.Close();
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

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
        }
    }
}