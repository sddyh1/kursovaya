using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using WpfApp1.Models;

namespace WpfApp1.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly string _connectionString;
        public event EventHandler? DataChanged;

        public TransactionRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            var transactions = new List<Transaction>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "SELECT * FROM Transactions ORDER BY TransactionDate DESC",
                    connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        transactions.Add(new Transaction
                        {
                            TransactionId = reader.GetInt32(0),
                            Description = reader.GetString(1),
                            Amount = reader.GetDecimal(2),
                            Type = reader.GetString(3),
                            CategoryName = reader.GetString(4),
                            TransactionDate = reader.GetDateTime(5),
                            UserId = reader.GetInt32(6)
                        });
                    }
                }
            }

            return transactions;
        }

        public async Task<Transaction> GetByIdAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "SELECT * FROM Transactions WHERE TransactionId = @id",
                    connection);
                command.Parameters.AddWithValue("@id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Transaction
                        {
                            TransactionId = reader.GetInt32(0),
                            Description = reader.GetString(1),
                            Amount = reader.GetDecimal(2),
                            Type = reader.GetString(3),
                            CategoryName = reader.GetString(4),
                            TransactionDate = reader.GetDateTime(5),
                            UserId = reader.GetInt32(6)
                        };
                    }
                }
            }

            return null;
        }

        public async Task AddAsync(Transaction transaction)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(@"
                    INSERT INTO Transactions (Description, Amount, Type, CategoryName, TransactionDate, UserId)
                    VALUES (@desc, @amount, @type, @category, @date, @userId)", connection);

                command.Parameters.AddWithValue("@desc", transaction.Description);
                command.Parameters.AddWithValue("@amount", transaction.Amount);
                command.Parameters.AddWithValue("@type", transaction.Type);
                command.Parameters.AddWithValue("@category", transaction.CategoryName);
                command.Parameters.AddWithValue("@date", transaction.TransactionDate);
                command.Parameters.AddWithValue("@userId", transaction.UserId);

                await command.ExecuteNonQueryAsync();
                OnDataChanged();
            }
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(@"
                    UPDATE Transactions 
                    SET Description = @desc, Amount = @amount, Type = @type, 
                        CategoryName = @category, TransactionDate = @date
                    WHERE TransactionId = @id", connection);

                command.Parameters.AddWithValue("@desc", transaction.Description);
                command.Parameters.AddWithValue("@amount", transaction.Amount);
                command.Parameters.AddWithValue("@type", transaction.Type);
                command.Parameters.AddWithValue("@category", transaction.CategoryName);
                command.Parameters.AddWithValue("@date", transaction.TransactionDate);
                command.Parameters.AddWithValue("@id", transaction.TransactionId);

                await command.ExecuteNonQueryAsync();
                OnDataChanged();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "DELETE FROM Transactions WHERE TransactionId = @id",
                    connection);
                command.Parameters.AddWithValue("@id", id);

                await command.ExecuteNonQueryAsync();
                OnDataChanged();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "SELECT COUNT(1) FROM Transactions WHERE TransactionId = @id",
                    connection);
                command.Parameters.AddWithValue("@id", id);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
        }

        public async Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId)
        {
            var transactions = new List<Transaction>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "SELECT * FROM Transactions WHERE UserId = @userId ORDER BY TransactionDate DESC",
                    connection);
                command.Parameters.AddWithValue("@userId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        transactions.Add(new Transaction
                        {
                            TransactionId = reader.GetInt32(0),
                            Description = reader.GetString(1),
                            Amount = reader.GetDecimal(2),
                            Type = reader.GetString(3),
                            CategoryName = reader.GetString(4),
                            TransactionDate = reader.GetDateTime(5),
                            UserId = reader.GetInt32(6)
                        });
                    }
                }
            }

            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetByCategoryAsync(string category)
        {
            var transactions = new List<Transaction>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "SELECT * FROM Transactions WHERE CategoryName = @category ORDER BY TransactionDate DESC",
                    connection);
                command.Parameters.AddWithValue("@category", category);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        transactions.Add(new Transaction
                        {
                            TransactionId = reader.GetInt32(0),
                            Description = reader.GetString(1),
                            Amount = reader.GetDecimal(2),
                            Type = reader.GetString(3),
                            CategoryName = reader.GetString(4),
                            TransactionDate = reader.GetDateTime(5),
                            UserId = reader.GetInt32(6)
                        });
                    }
                }
            }

            return transactions;
        }

        public async Task<decimal> GetTotalIncomeAsync(int userId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "SELECT COALESCE(SUM(Amount), 0) FROM Transactions WHERE UserId = @userId AND Type = 'Income'",
                    connection);
                command.Parameters.AddWithValue("@userId", userId);

                var result = await command.ExecuteScalarAsync();
                return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
            }
        }

        public async Task<decimal> GetTotalExpensesAsync(int userId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "SELECT COALESCE(SUM(Amount), 0) FROM Transactions WHERE UserId = @userId AND Type = 'Expense'",
                    connection);
                command.Parameters.AddWithValue("@userId", userId);

                var result = await command.ExecuteScalarAsync();
                return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
            }
        }

        protected virtual void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}