using System;
using System.Linq;
using System.Windows;
using WpfApp1.Models;
using WpfApp1.Repositories;
using WpfApp1.Views;

namespace WpfApp1
{
    public partial class AdminWindow : Window
    {
        private readonly IUserRepository _userRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly User _currentAdmin;

        public AdminWindow(User admin)
        {
            if (admin is null)
                throw new ArgumentNullException(nameof(admin), "Admin cannot be null");

            InitializeComponent();
            _currentAdmin = admin;
            _userRepository = new UserRepository("Data Source=finance.db;Version=3;");
            _transactionRepository = new TransactionRepository("Data Source=finance.db;Version=3;");

            // Подписываемся на события изменения данных
            _userRepository.DataChanged += async (s, e) => await LoadStatisticsAsync();
            _transactionRepository.DataChanged += async (s, e) => await LoadStatisticsAsync();

            InitializeUI();
            _ = LoadStatisticsAsync(); // Загружаем статистику асинхронно

            // По умолчанию показываем управление пользователями
            ManageUsersBtn_Click(null, null);
        }

        private void InitializeUI()
        {
            AdminUserText.Text = $"Администратор: {_currentAdmin.Username}";
            Title = $"Панель администратора - {_currentAdmin.Username}";
        }

        private async System.Threading.Tasks.Task LoadStatisticsAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var transactions = await _transactionRepository.GetAllAsync();

                // Используем Dispatcher для обновления UI из другого потока
                await Dispatcher.InvokeAsync(() =>
                {
                    TotalUsersText.Text = $"Всего пользователей: {users.Count()}";
                    TotalTransactionsText.Text = $"Всего транзакций: {transactions.Count()}";

                    var totalIncome = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
                    var totalExpenses = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
                    var totalBalance = totalIncome + totalExpenses;

                    TotalBalanceText.Text = $"Общий баланс: {totalBalance:N2} ₽";
                });
            }
            catch (Exception ex)
            {
                // Выводим ошибку в UI потоке
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private void ManageUsersBtn_Click(object sender, RoutedEventArgs e)
        {
            var usersPage = new AdminUsersPage(_userRepository, _transactionRepository);
            ContentFrame.Navigate(usersPage);
        }

        private void ManageTransactionsBtn_Click(object sender, RoutedEventArgs e)
        {
            var transactionsPage = new AdminTransactionsPage(_userRepository, _transactionRepository);
            ContentFrame.Navigate(transactionsPage);
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из панели администратора?",
                "Подтверждение выхода", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}