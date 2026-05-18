using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Учет.Data.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(object id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task AddOrUpdateAsync(T entity);
    }
}