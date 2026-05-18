using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Учет.Core;
using Учет.Data.Interfaces;
using Учет.Enums;
using Учет.Models;
using Учет.Services;

namespace Учет.ViewModels
{
    public class AssetEditViewModel : BaseViewModel, INotifyDataErrorInfo
    {
        private readonly IValidationService _validation;
        private readonly IUnitOfWork _uow;
        private readonly Asset _asset;
        private readonly bool _isEdit;
        private readonly Dictionary<string, List<string>> _errors = new();

        public ObservableCollection<Department> Departments { get; } = new();
        public ObservableCollection<AssetType> AssetTypes { get; } = new();

        private string _shortName = string.Empty;
        public string ShortName
        {
            get => _shortName;
            set => SetAndValidate(ref _shortName, value);
        }

        private string _fullName = string.Empty;
        public string FullName
        {
            get => _fullName;
            set => SetAndValidate(ref _fullName, value);
        }

        private string _inventoryNumber = string.Empty;
        public string InventoryNumber
        {
            get => _inventoryNumber;
            set => SetAndValidate(ref _inventoryNumber, value);
        }

        private DateTime? _commissioningDate = DateTime.Today;
        public DateTime? CommissioningDate
        {
            get => _commissioningDate;
            set => SetAndValidate(ref _commissioningDate, value);
        }

        private int _departmentId;
        public int DepartmentId
        {
            get => _departmentId;
            set => SetAndValidate(ref _departmentId, value);
        }

        private int _assetTypeId;
        public int AssetTypeId
        {
            get => _assetTypeId;
            set => SetAndValidate(ref _assetTypeId, value);
        }

        public bool IsEditMode => _isEdit;
        public string WindowTitle => _isEdit ? "Редактирование устройства" : "Добавление устройства";

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public AssetEditViewModel(IValidationService validation, IUnitOfWork uow, Asset? assetToEdit = null)
        {
            _validation = validation;
            _uow = uow;
            _asset = assetToEdit ?? new Asset();
            _isEdit = assetToEdit != null;

            if (assetToEdit != null)
            {
                _shortName = assetToEdit.ShortName;
                _fullName = assetToEdit.FullName;
                _inventoryNumber = assetToEdit.InventoryNumber;
                _commissioningDate = assetToEdit.CommissioningDate;
                _departmentId = assetToEdit.DepartmentId;
                _assetTypeId = assetToEdit.AssetTypeId;

            }

            LoadLookups();

            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !HasErrors);
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private void SetAndValidate<T>(ref T field, T value)
        {
            if (Equals(field, value)) return;
            field = value;
            OnPropertyChanged();
            Validate();
        }

        private void Validate()
        {
            _errors.Clear();

            var validationErrors = _validation.ValidateLocal(new Asset
            {
                ShortName = ShortName,
                InventoryNumber = InventoryNumber,
                CommissioningDate = CommissioningDate ?? DateTime.Today,
                DepartmentId = DepartmentId
            });

            foreach (var err in validationErrors)
            {
                if (!_errors.ContainsKey(err.Key))
                    _errors[err.Key] = new List<string>();
                _errors[err.Key].Add(err.Value);
                OnErrorsChanged(err.Key);
            }

            SaveCommand.RaiseCanExecuteChanged();
        }

        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        private async void LoadLookups()
        {
            var depts = await _uow.Departments.GetAllAsync();
            Departments.Clear();
            foreach (var d in depts) Departments.Add(d);

            var types = await _uow.AssetTypes?.GetAllAsync() ?? new List<AssetType>();
            AssetTypes.Clear();
            foreach (var t in types) AssetTypes.Add(t);
        }

        private async Task SaveAsync()
        {
            if (HasErrors) return;

            if (!await _validation.IsInventoryUniqueAsync(InventoryNumber, _isEdit ? _asset.Id : null))
            {
                if (!_errors.ContainsKey(nameof(InventoryNumber)))
                    _errors[nameof(InventoryNumber)] = new List<string>();
                _errors[nameof(InventoryNumber)].Add("Инвентарный номер уже существует");
                OnErrorsChanged(nameof(InventoryNumber));
                return;
            }

            _asset.ShortName = ShortName;
            _asset.FullName = FullName;
            _asset.InventoryNumber = InventoryNumber;
            _asset.CommissioningDate = CommissioningDate ?? DateTime.Today;
            _asset.DepartmentId = DepartmentId;
            _asset.AssetTypeId = AssetTypeId;
            _asset.PlannedWriteOffDate = _asset.CommissioningDate.AddYears(5);

            if (!_isEdit)
                _asset.Status = (int)AssetStatus.Registered;

            await _uow.Assets.AddOrUpdateAsync(_asset);
            await _uow.CompleteAsync();

            MessageBox.Show("Данные успешно сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            Cancel();
        }

        private void Cancel()
        {
            if (Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive) is Window win)
                win.Close();
        }

        #region INotifyDataErrorInfo
        public bool HasErrors => _errors.Count > 0;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return _errors.Values.SelectMany(v => v);
            return _errors.TryGetValue(propertyName, out var errs) ? errs : Enumerable.Empty<string>();
        }
        #endregion
        public void SetForEdit(Asset asset)
        {
            if (asset == null) return;

            _asset.Id = asset.Id;
            _asset.ShortName = asset.ShortName;
            _asset.FullName = asset.FullName;
            _asset.InventoryNumber = asset.InventoryNumber;
            _asset.CommissioningDate = asset.CommissioningDate;
            _asset.PlannedWriteOffDate = asset.PlannedWriteOffDate;
            _asset.Status = asset.Status;
            _asset.RepairCount = asset.RepairCount;
            _asset.DepartmentId = asset.DepartmentId;
            _asset.AssetTypeId = asset.AssetTypeId;

            ShortName = asset.ShortName;
            FullName = asset.FullName;
            InventoryNumber = asset.InventoryNumber;
            CommissioningDate = asset.CommissioningDate;
            DepartmentId = asset.DepartmentId;
            AssetTypeId = asset.AssetTypeId;
        }
    }
}