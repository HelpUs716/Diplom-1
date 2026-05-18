using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows;
using Учет.Core;
using Учет.Data;
using Учет.Data.Interfaces;
using Учет.Data.Repositories;
using Учет.Enums;
using Учет.Models;
using Учет.Services;
using Учет.ViewModels;
using Учет.Views;

namespace Учет
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            ConfigureServices(services, configuration);

            ServiceProvider = services.BuildServiceProvider();

            // СОЗДАЁМ БД АВТОМАТИЧЕСКИ ПРИ ЗАПУСКЕ
            using (var scope = ServiceProvider.CreateScope())
            {
                var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<УчетDbContext>>();
                using var context = factory.CreateDbContext();
                context.Database.EnsureCreated();

                // Добавляем тестовые данные, если таблицы пустые
                if (!context.Departments.Any())
                {
                    context.Departments.AddRange(
                        new Department { Name = "Администрация" },
                        new Department { Name = "IT Отдел" },
                        new Department { Name = "Бухгалтерия" }
                    );

                    if (!context.AssetTypes.Any())
                    {
                        context.AssetTypes.AddRange(
                            new AssetType { Name = "ПК", DefaultWarrantyPeriod = 5 },
                            new AssetType { Name = "Принтер", DefaultWarrantyPeriod = 3 }
                        );
                    }

                    context.SaveChanges();

                    if (!context.Assets.Any())
                    {
                        var adminDept = context.Departments.FirstOrDefault(d => d.Name == "Администрация");
                        var pcType = context.AssetTypes.FirstOrDefault(t => t.Name == "ПК");

                        if (adminDept != null && pcType != null)
                        {
                            context.Assets.Add(new Asset
                            {
                                ShortName = "Тест ПК",
                                FullName = "HP ProDesk 400 G6",
                                InventoryNumber = "INV-001",
                                CommissioningDate = DateTime.Today,
                                PlannedWriteOffDate = DateTime.Today.AddYears(5),
                                Status = (int)AssetStatus.InService,
                                RepairCount = 0,
                                DepartmentId = adminDept.Id,
                                AssetTypeId = pcType.Id
                            });
                            context.SaveChanges();
                        }
                    }
                }
            }

            // ПОКАЗЫВАЕМ ГЛАВНОЕ ОКНО
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Database - используем фабрику для создания новых экземпляров DbContext
            services.AddDbContextFactory<УчетDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Repositories & UnitOfWork - Transient (новый экземпляр каждый раз)
            services.AddTransient<IUnitOfWork, UnitOfWork>();

            // Services
            services.AddScoped<IValidationService, ValidationService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<IUserSessionService, UserSessionService>();

            // ViewModels & Views
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<MainWindow>();
            services.AddTransient<AssetEditViewModel>();
            services.AddTransient<AssetEditWindow>();
            services.AddTransient<AuthViewModel>();
            services.AddTransient<AuthWindow>();
            services.AddTransient<RepairHistoryViewModel>();
            services.AddTransient<RepairHistoryWindow>();
            services.AddTransient<DepartmentManagementViewModel>();
            //services.AddTransient<DepartmentManagementWindow>();
            services.AddTransient<DepartmentEditViewModel>();
            services.AddTransient<DepartmentEditWindow>();
            services.AddTransient<DocumentTypeSelectorViewModel>();
            services.AddTransient<DocumentTypeSelectorWindow>();
        }
    }
}