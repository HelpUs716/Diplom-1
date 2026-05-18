using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Учет.Core;
using Учет.Data.Interfaces;
using Учет.Models;
using Учет.Services;
using Учет.Views;

namespace Учет.ViewModels
{
    public class DepartmentManagementViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _uow;
        private readonly IDepartmentService _deptService;
        public ObservableCollection<Department> Departments { get; } = new();
        private Department? _selected;
        public Department? SelectedDepartment { get => _selected; set { _selected = value; OnPropertyChanged(); RefreshCmd(); } }
        public bool HasSelection => SelectedDepartment != null;

        public RelayCommand AddCmd, EditCmd, DelCmd, CloseCmd;

        public DepartmentManagementViewModel(IUnitOfWork uow, IDepartmentService deptService)
        {
            _uow = uow; _deptService = deptService;
            AddCmd = new RelayCommand(_ => OpenEdit(null));
            EditCmd = new RelayCommand(_ => OpenEdit(SelectedDepartment), _ => HasSelection);
            DelCmd = new RelayCommand(_ => DeleteAsync().FireAndForgetSafeAsync(), _ => HasSelection);
            CloseCmd = new RelayCommand(_ => CloseWindow());
            LoadAsync().FireAndForgetSafeAsync();
        }

        private async Task LoadAsync()
        {
            var flat = await _uow.Departments.GetAllAsync();
            var tree = _deptService.BuildHierarchy(flat);
            Departments.Clear();
            foreach (var d in tree) Departments.Add(d);
        }

        private void OpenEdit(Department? dept)
        {
            var win = new DepartmentEditWindow(dept) { Owner = Application.Current.MainWindow };

            if (win.ShowDialog() == true)
                LoadAsync().FireAndForgetSafeAsync();
            if (win.ShowDialog() == true) LoadAsync().FireAndForgetSafeAsync();
        }

        private async Task DeleteAsync()
        {
            if (SelectedDepartment == null) return;
            if (await _deptService.HasDependenciesAsync(SelectedDepartment.Id))
            {
                MessageBox.Show("Нельзя удалить отдел: в нём есть техника или вложенные отделы.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (MessageBox.Show($"Удалить \"{SelectedDepartment.Name}\"?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _uow.Departments.DeleteAsync(SelectedDepartment);
                await _uow.CompleteAsync();
                await LoadAsync();
            }
        }

        private void CloseWindow()
        {
            if (Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) is Window win)
                win.Close();
        }

        private void RefreshCmd() => CommandManager.InvalidateRequerySuggested();
    }
}