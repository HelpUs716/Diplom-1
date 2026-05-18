using Microsoft.EntityFrameworkCore;
using Учет.Data.Interfaces;
using Учет.Models;

namespace Учет.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly УчетDbContext _context;

        public UnitOfWork(IDbContextFactory<УчетDbContext> factory)
        {
            _context = factory.CreateDbContext();
        }

        public IRepository<Asset> Assets => new Repository<Asset>(_context);
        public IRepository<Department> Departments => new Repository<Department>(_context);
        public IRepository<Repair> Repairs => new Repository<Repair>(_context);
        public IRepository<User> Users => new Repository<User>(_context);
        public IRepository<AssetType> AssetTypes => new Repository<AssetType>(_context);

        public Task<int> CompleteAsync() => _context.SaveChangesAsync();

        public void Dispose() => _context?.Dispose();
    }
}