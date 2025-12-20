using System.Windows;

namespace WpfApp1
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Инициализируем базу данных при запуске приложения
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                // Создаем контекст базы данных, который автоматически инициализирует БД
                var dbContext = new Data.DatabaseContext("Data Source=finance.db;Version=3;");
            }
            catch
            {
                // Ошибка уже обработана в DatabaseContext
            }
        }
    }
}