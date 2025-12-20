using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using WpfApp1.Models;

namespace WpfApp1.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        public event EventHandler? DataChanged; // Реализуем событие

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var users = new List<User>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM Users", connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new User
                        {
                            UserId = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            PasswordHash = reader.GetString(2),
                            Email = reader.GetString(3),
                            Role = reader.GetString(4),
                            CreatedAt = reader.GetDateTime(5),
                            IsActive = reader.GetBoolean(6)
                        });
                    }
                }
            }

            return users;
        }

        public async Task<User> GetByIdAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM Users WHERE UserId = @id", connection);
                command.Parameters.AddWithValue("@id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            UserId = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            PasswordHash = reader.GetString(2),
                            Email = reader.GetString(3),
                            Role = reader.GetString(4),
                            CreatedAt = reader.GetDateTime(5),
                            IsActive = reader.GetBoolean(6)
                        };
                    }
                }
            }

            return null;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM Users WHERE Username = @username", connection);
                command.Parameters.AddWithValue("@username", username);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            UserId = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            PasswordHash = reader.GetString(2),
                            Email = reader.GetString(3),
                            Role = reader.GetString(4),
                            CreatedAt = reader.GetDateTime(5),
                            IsActive = reader.GetBoolean(6)
                        };
                    }
                }
            }

            return null;
        }

        public async Task AddAsync(User user)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(@"
                    INSERT INTO Users (Username, PasswordHash, Email, Role, CreatedAt, IsActive)
                    VALUES (@username, @password, @email, @role, @createdAt, @isActive)", connection);

                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@password", user.PasswordHash);
                command.Parameters.AddWithValue("@email", user.Email);
                command.Parameters.AddWithValue("@role", user.Role);
                command.Parameters.AddWithValue("@createdAt", user.CreatedAt);
                command.Parameters.AddWithValue("@isActive", user.IsActive);

                await command.ExecuteNonQueryAsync();
                OnDataChanged(); // Уведомляем об изменении
            }
        }

        public async Task UpdateAsync(User user)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(@"
                    UPDATE Users 
                    SET Username = @username, PasswordHash = @password, Email = @email, 
                        Role = @role, IsActive = @isActive
                    WHERE UserId = @id", connection);

                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@password", user.PasswordHash);
                command.Parameters.AddWithValue("@email", user.Email);
                command.Parameters.AddWithValue("@role", user.Role);
                command.Parameters.AddWithValue("@isActive", user.IsActive);
                command.Parameters.AddWithValue("@id", user.UserId);

                await command.ExecuteNonQueryAsync();
                OnDataChanged(); // Уведомляем об изменении
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("DELETE FROM Users WHERE UserId = @id", connection);
                command.Parameters.AddWithValue("@id", id);

                await command.ExecuteNonQueryAsync();
                OnDataChanged(); // Уведомляем об изменении
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT COUNT(1) FROM Users WHERE UserId = @id", connection);
                command.Parameters.AddWithValue("@id", id);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var user = await GetByUsernameAsync(username);
            if (user == null || !user.IsActive)
                return false;

            return user.PasswordHash == password;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT COUNT(1) FROM Users WHERE Username = @username", connection);
                command.Parameters.AddWithValue("@username", username);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
        }

        protected virtual void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}