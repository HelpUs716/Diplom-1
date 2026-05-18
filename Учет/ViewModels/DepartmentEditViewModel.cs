using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Учет.Core;
using Учет.Data.Interfaces;
using Учет.Models;
using Учет.Services;

namespace Учет.ViewModels
{
    public class DepartmentEditViewModel : BaseViewModel, INotifyDataErrorInfo
    {
        private readonly IDepartmentService _deptService;
        private readonly IUnitOfWork _uow;
        private Department _dept = new();
        private bool _isEdit = false;
        private readonly Dictionary<string, List<string>> _errors = new();

        public string WindowTitle => _isEdit ? "Редактирование отдела" : "Новый отдел";
        public ObservableCollection<Department> AllDepartments { get; } = new();

        public string Name { get => _dept.Name; set => SetAndValidate(v => _dept.Name = v, value); }
        public int? ParentId { get => _dept.ParentDepartmentId; set => SetAndValidate(v => _dept.ParentDepartmentId = v, value); }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public DepartmentEditViewModel(IDepartmentService deptService, IUnitOfWork uow, Department? dept = null)
        {
            _deptService = deptService; _uow = uow;
            if (dept != null) { _dept = dept; _isEdit = true; }
            LoadParents();
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !HasErrors);
            CancelCommand = new RelayCommand(_ => Close(false));
        }

        private async void LoadParents()
        {
            var list = await _uow.Departments.GetAllAsync();
            AllDepartments.Clear();
            foreach (var d in list) AllDepartments.Add(d);
        }

        private void SetAndValidate<T>(Action<T> setter, T value)
        {
            setter(value); OnPropertyChanged(); ClearErrors(nameof(Name)); ClearErrors(nameof(ParentId));
            if (string.IsNullOrWhiteSpace(Name)) AddError(nameof(Name), "Название обязательно");
            SaveCommand.RaiseCanExecuteChanged();
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name)) { AddError(nameof(Name), "Название обязательно"); return; }
            if (!await _deptService.IsNameUniqueAsync(Name, ParentId, _isEdit ? _dept.Id : null))
            {
                AddError(nameof(Name), "Отдел с таким именем уже существует в данном узле");
                return;
            }

            if (!_isEdit) await _uow.Departments.AddAsync(_dept);
            await _uow.Departments.UpdateAsync(_dept);

            await _uow.CompleteAsync();
            Close(true);
        }

        private void Close(bool result)
        {
            if (Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) is Window win)
                win.DialogResult = result;
        }

        #region INotifyDataErrorInfo
        public bool HasErrors => _errors.Count > 0;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public IEnumerable GetErrors(string? propertyName) => propertyName != null && _errors.TryGetValue(propertyName, out var e) ? e : Enumerable.Empty<string>();
        private void AddError(string prop, string err) { if (!_errors.ContainsKey(prop)) _errors[prop] = new(); if (!_errors[prop].Contains(err)) { _errors[prop].Add(err); ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop)); } }
        private void ClearErrors(string prop) { if (_errors.Remove(prop)) ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop)); }
        #endregion
    }
}