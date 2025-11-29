using System;
using System.Data.SQLite;
using System.IO;
using System.Windows;

namespace WpfApp1.Data
{
    public class DatabaseContext
    {
        private readonly string _connectionString;

        public DatabaseContext(string connectionString)
        {
            _connectionString = connectionString;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                bool databaseCreated = !File.Exists("finance.db");

                if (databaseCreated)
                {
                    SQLiteConnection.CreateFile("finance.db");
                }

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    var command = new SQLiteCommand(@"
                        CREATE TABLE IF NOT EXISTS Users (
                            UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                            Username TEXT NOT NULL UNIQUE,
                            PasswordHash TEXT NOT NULL,
                            Email TEXT NOT NULL,
                            Role TEXT NOT NULL DEFAULT 'User',
                            CreatedAt DATETIME NOT NULL,
                            IsActive BOOLEAN NOT NULL DEFAULT 1
                        )", connection);
                    command.ExecuteNonQuery();

                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Transactions (
                            TransactionId INTEGER PRIMARY KEY AUTOINCREMENT,
                            Description TEXT NOT NULL,
                            Amount DECIMAL(10,2) NOT NULL,
                            Type TEXT NOT NULL,
                            CategoryName TEXT NOT NULL,
                            TransactionDate DATETIME NOT NULL,
                            UserId INTEGER NOT NULL,
                            FOREIGN KEY (UserId) REFERENCES Users(UserId)
                        )";
                    command.ExecuteNonQuery();

                    if (databaseCreated)
                    {
                        command.CommandText = @"
                            INSERT INTO Users (Username, PasswordHash, Email, Role, CreatedAt, IsActive)
                            VALUES ('admin', 'admin123', 'admin@finance.com', 'Admin', datetime('now'), 1)";
                        command.ExecuteNonQuery();

                        command.CommandText = @"
                            INSERT INTO Transactions (Description, Amount, Type, CategoryName, TransactionDate, UserId)
                            VALUES 
                            ('Зарплата', 50000.00, 'Income', 'Salary', datetime('now', '-7 days'), 1),
                            ('Продукты', -3500.50, 'Expense', 'Food', datetime('now', '-6 days'), 1),
                            ('Коммунальные услуги', -2500.00, 'Expense', 'Utilities', datetime('now', '-5 days'), 1),
                            ('Фриланс', 15000.00, 'Income', 'Freelance', datetime('now', '-4 days'), 1)";
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации базы данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}