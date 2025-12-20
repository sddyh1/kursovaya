using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WpfApp1.Models;

namespace WpfApp1.Repositories
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Transaction>> GetByCategoryAsync(string category);
        Task<decimal> GetTotalIncomeAsync(int userId);
        Task<decimal> GetTotalExpensesAsync(int userId);
        event EventHandler DataChanged;  // Убедитесь, что это есть
    }
}