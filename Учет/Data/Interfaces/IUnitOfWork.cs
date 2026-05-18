using System;
using System.Collections.Generic;
using System.Text;
using Учет.Models;

namespace Учет.Data.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Asset> Assets { get; }
        IRepository<Department> Departments { get; }
        IRepository<Repair> Repairs { get; }
        IRepository<User> Users { get; }
        IRepository<AssetType> AssetTypes { get; } 
        Task<int> CompleteAsync();
    }
}