using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Учет.Data.Interfaces;

namespace Учет.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<List<T>> GetAllAsync() => await _dbSet.ToListAsync().ConfigureAwait(false);

        public async Task<T?> GetByIdAsync(object id) => await _dbSet.FindAsync(id).ConfigureAwait(false);

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity).ConfigureAwait(false);

        public Task UpdateAsync(T entity) { _dbSet.Update(entity); return Task.CompletedTask; }

        public Task DeleteAsync(T entity) { _dbSet.Remove(entity); return Task.CompletedTask; }

        public async Task AddOrUpdateAsync(T entity)
        {
            var pk = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey();
            if (pk == null) { await AddAsync(entity); return; }

            var keyProp = pk.Properties.First();
            var keyValue = keyProp.PropertyInfo?.GetValue(entity);

            if (keyValue == null || (int)keyValue == 0)
                await AddAsync(entity);
            else
                UpdateAsync(entity);
        }
    }
}