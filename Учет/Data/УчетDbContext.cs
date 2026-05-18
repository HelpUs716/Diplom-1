using Microsoft.EntityFrameworkCore;
using Учет.Models;

namespace Учет.Data
{
    public partial class УчетDbContext : DbContext
    {
        public УчетDbContext(DbContextOptions<УчетDbContext> options) : base(options) { }

        public DbSet<Asset> Assets { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Repair> Repairs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AssetType> AssetTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Department>()
                .HasOne(d => d.ParentDepartment)
                .WithMany(d => d.InverseParentDepartment)
                .HasForeignKey(d => d.ParentDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Asset>()
                .HasIndex(a => a.InventoryNumber)
                .IsUnique();
        }
    }
}