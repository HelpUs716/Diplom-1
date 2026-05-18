using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Учет.Core;
using Учет.Data.Interfaces;
using Учет.Enums;
using Учет.Models;
using Учет.Services;
using Учет.Views;

namespace Учет.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly IUserSessionService _session;
        private readonly IDocumentService _docService;
        private readonly IServiceProvider _sp;

        public ObservableCollection<Department> Departments { get; } = new();
        public ObservableCollection<Asset> Assets { get; } = new();
        public ICollectionView AssetsView { get; }

        private string _search = string.Empty;
        public string Search
        {
            get => _search;
            set { _search = value; OnPropertyChanged(); AssetsView?.Refresh(); }
        }

        private string _filterStatus = "Все";
        public string FilterStatus
        {
            get => _filterStatus;
            set { _filterStatus = value; OnPropertyChanged(); AssetsView?.Refresh(); }
        }

        public IEnumerable<string> StatusList => new[] { "Все" }.Concat(Enum.GetNames(typeof(AssetStatus)));

        private Department? _selectedDept;
        public Department? SelectedDepartment
        {
            get => _selectedDept;
            set
            {
                _selectedDept = value;
                OnPropertyChanged();
                _ = LoadAssetsAsync();
            }
        }

        private Asset? _selectedAsset;
        public Asset? SelectedAsset
        {
            get => _selectedAsset;
            set { _selectedAsset = value; OnPropertyChanged(); RefreshCommands(); }
        }

        public bool IsAdmin => true; //РОФЛС

        public RelayCommand AddCmd { get; }
        public RelayCommand EditCmd { get; }
        public RelayCommand DelCmd { get; }
        public RelayCommand RepairCmd { get; }
        public RelayCommand ReturnCmd { get; }
        public RelayCommand DocCmd { get; }
        public RelayCommand HistCmd { get; }
        public RelayCommand ExportCmd { get; }
        public RelayCommand AddDepartmentCommand { get; }

        public MainWindowViewModel(IUserSessionService session, IDocumentService docService, IServiceProvider sp)
        {
            _session = session;
            _docService = docService;
            _sp = sp;

            AssetsView = CollectionViewSource.GetDefaultView(Assets);
            AssetsView.Filter = o => o is Asset a && MatchSearch(a) && MatchStatus(a);

            AddCmd = new RelayCommand(_ => OpenAssetEdit(), _ => IsAdmin);
            EditCmd = new RelayCommand(_ => OpenAssetEdit(SelectedAsset), _ => IsAdmin && SelectedAsset != null);
            DelCmd = new RelayCommand(_ => DeleteAsset(), _ => IsAdmin && SelectedAsset != null);
            RepairCmd = new RelayCommand(_ => SendToRepair(), _ => IsAdmin && SelectedAsset?.Status == (int)AssetStatus.InService);
            ReturnCmd = new RelayCommand(_ => ReturnFromRepair(), _ => IsAdmin && SelectedAsset?.Status == (int)AssetStatus.UnderRepair);
            DocCmd = new RelayCommand(_ => GenerateDocument(), _ => SelectedAsset != null);
            HistCmd = new RelayCommand(_ => OpenRepairHistory(), _ => SelectedAsset != null);
            ExportCmd = new RelayCommand(_ => ExportToCsv(), _ => AssetsView.Cast<Asset>().Any());
            AddDepartmentCommand = new RelayCommand(_ => OpenAddDepartment(), _ => IsAdmin);

            Task.Run(async () => await LoadDepartmentsAsync());
        }

        private bool MatchSearch(Asset a) => string.IsNullOrWhiteSpace(Search) ||
            a.InventoryNumber.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
            a.ShortName.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
            a.FullName.Contains(Search, StringComparison.OrdinalIgnoreCase);

        private bool MatchStatus(Asset a) => FilterStatus == "Все" || a.Status.ToString() == FilterStatus;
        private void RefreshCommands() => CommandManager.InvalidateRequerySuggested();


        public async Task LoadDepartmentsAsync()
        {
            try
            {
                using var scope = _sp.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var flatList = (await uow.Departments.GetAllAsync()).ToList();

                var tree = BuildHierarchy(flatList, null);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Departments.Clear();
                    foreach (var d in tree) Departments.Add(d);
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки отделов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private List<Department> BuildHierarchy(List<Department> all, int? parentId)
        {
            return all.Where(d => d.ParentDepartmentId == parentId)
                      .Select(d =>
                      {
                          d.Children = new ObservableCollection<Department>(BuildHierarchy(all, d.Id));
                          return d;
                      }).ToList();
        }

        private async Task LoadAssetsAsync()
        {
            try
            {
                if (SelectedDepartment == null) return;

                using var scope = _sp.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var all = await uow.Assets.GetAllAsync().ConfigureAwait(false);
                var filtered = all.Where(a => a.DepartmentId == SelectedDepartment.Id).ToList();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Assets.Clear();
                    foreach (var a in filtered) Assets.Add(a);
                    AssetsView.Refresh();
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки активов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private async void OpenAssetEdit(Asset? asset = null)
        {
            using var scope = _sp.CreateScope();
            var vm = scope.ServiceProvider.GetRequiredService<AssetEditViewModel>();
            if (asset != null) vm.SetForEdit(asset);
            var win = scope.ServiceProvider.GetRequiredService<AssetEditWindow>();
            win.Owner = Application.Current.MainWindow;
            if (win.ShowDialog() == true) await LoadAssetsAsync();
        }

        private async void DeleteAsset()
        {
            if (SelectedAsset == null) return;
            if (MessageBox.Show("Удалить объект?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            using var scope = _sp.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await uow.Assets.DeleteAsync(SelectedAsset);
            await uow.CompleteAsync();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Assets.Remove(SelectedAsset);
                SelectedAsset = null;
            });
        }

        private async void SendToRepair()
        {
            if (SelectedAsset == null) return;

            using var scope = _sp.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            SelectedAsset.Status = (int)AssetStatus.UnderRepair;
            await uow.Repairs.AddAsync(new Repair
            {
                AssetId = SelectedAsset.Id,
                StartDate = DateTime.Now,
                Description = "Отправлено на ремонт"
            });
            await uow.CompleteAsync();
            RefreshCommands();
        }

        private async void ReturnFromRepair()
        {
            if (SelectedAsset == null) return;

            using var scope = _sp.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var all = await uow.Repairs.GetAllAsync().ConfigureAwait(false);
            var active = all.FirstOrDefault(r => r.AssetId == SelectedAsset.Id && r.EndDate == null);
            if (active != null) active.EndDate = DateTime.Now;
            SelectedAsset.Status = (int)AssetStatus.InService;
            SelectedAsset.RepairCount++;
            await uow.CompleteAsync();
            RefreshCommands();
        }

        private void GenerateDocument()
        {
            if (SelectedAsset == null || SelectedDepartment == null) return;
            var selector = new DocumentTypeSelectorWindow { Owner = Application.Current.MainWindow };
            if (selector.ShowDialog() != true) return;

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                var tplPath = Path.Combine(AppContext.BaseDirectory, "Templates");
                _docService.Generate(SelectedAsset, SelectedDepartment.Name, selector.SelectedType, tplPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка генерации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void OpenAddDepartment()
        {
            var win = new DepartmentEditWindow();
            win.Owner = Application.Current.MainWindow;
            if (win.ShowDialog() == true)
            {
                Task.Run(async () => await LoadDepartmentsAsync());
            }
        }

        private void OpenRepairHistory()
        {
            if (SelectedAsset == null) return;

            using var scope = _sp.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var vm = new RepairHistoryViewModel(uow, SelectedAsset.Id, SelectedAsset.ShortName);
            var win = new RepairHistoryWindow(vm) { Owner = Application.Current.MainWindow };
            win.ShowDialog();
        }

        private void ExportToCsv()
        {
            var assets = AssetsView?.Cast<Asset>().ToList();
            if (assets == null || !assets.Any())
            {
                MessageBox.Show("Нет данных для экспорта.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV файлы (*.csv)|*.csv",
                FileName = $"Учет_{DateTime.Now:yyyyMMdd}.csv",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (dlg.ShowDialog() != true) return;

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("Инв.номер;Краткое имя;Модель;Отдел;Статус;Дата ввода;План.списание;Ремонтов");

                foreach (var a in assets)
                {
                    var dept = Departments.FirstOrDefault(d => d.Id == a.DepartmentId)?.Name ?? "Не указан";
                    sb.AppendLine($"\"{a.InventoryNumber}\";\"{a.ShortName}\";\"{a.FullName}\";{dept};{a.Status};{a.CommissioningDate:dd.MM.yyyy};{a.PlannedWriteOffDate:dd.MM.yyyy};{a.RepairCount}");
                }

                File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));
                MessageBox.Show("Экспорт завершён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}