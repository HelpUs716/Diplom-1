using System;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Учет.Data.Interfaces;
using Учет.Enums;
using Учет.Models;

namespace Учет.Views
{
    public partial class AssetEditWindow : Window
    {
        private readonly Asset _asset;
        private readonly bool _isEdit;

        public AssetEditWindow(Asset asset = null)
        {
            InitializeComponent();
            _asset = asset ?? new Asset();
            _isEdit = asset != null && asset.Id > 0;

            if (_isEdit)
                Title = "✏️ Редактирование оборудования";

            LoadData();
        }

        private async void LoadData()
        {
            using var scope = App.ServiceProvider.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Загрузка отделов
            var departments = await uow.Departments.GetAllAsync();
            DepartmentCombo.ItemsSource = departments;
            DepartmentCombo.DisplayMemberPath = "Name";
            DepartmentCombo.SelectedValuePath = "Id";

            // Загрузка статусов
            StatusCombo.ItemsSource = Enum.GetValues(typeof(AssetStatus));

            if (_isEdit)
            {
                ShortNameBox.Text = _asset.ShortName;
                FullNameBox.Text = _asset.FullName;
                InventoryBox.Text = _asset.InventoryNumber;
                DatePicker.SelectedDate = _asset.CommissioningDate;
                DepartmentCombo.SelectedValue = _asset.DepartmentId;
                StatusCombo.SelectedItem = (AssetStatus)_asset.Status;
                TypeBox.Text = _asset.AssetType?.Name ?? "ПК";
            }
            else
            {
                DatePicker.SelectedDate = DateTime.Today;
                StatusCombo.SelectedItem = AssetStatus.Registered;
                TypeBox.Text = "ПК";
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ShortNameBox.Text))
            {
                MessageBox.Show("Введите краткое название!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(InventoryBox.Text))
            {
                MessageBox.Show("Введите инвентарный номер!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _asset.ShortName = ShortNameBox.Text.Trim();
            _asset.FullName = FullNameBox.Text.Trim();
            _asset.InventoryNumber = InventoryBox.Text.Trim();
            _asset.CommissioningDate = DatePicker.SelectedDate ?? DateTime.Today;
            _asset.PlannedWriteOffDate = _asset.CommissioningDate.AddYears(5);

            if (DepartmentCombo.SelectedValue != null)
                _asset.DepartmentId = (int)DepartmentCombo.SelectedValue;

            _asset.Status = (int)(StatusCombo.SelectedItem as AssetStatus? ?? AssetStatus.Registered);
            _asset.AssetType = new AssetType { Name = TypeBox.Text.Trim() };

            using var scope = App.ServiceProvider.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            if (_isEdit)
                await uow.Assets.UpdateAsync(_asset);
            else
                await uow.Assets.AddAsync(_asset);

            await uow.CompleteAsync();

            DialogResult = true;
            Close();
        }
    }
}