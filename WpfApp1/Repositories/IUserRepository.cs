using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WpfApp1.Models;

namespace WpfApp1.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> ValidateUserAsync(string username, string password);
        Task<bool> UsernameExistsAsync(string username);

        // Добавляем событие для уведомления об изменении данных
        event EventHandler DataChanged;
    }
}