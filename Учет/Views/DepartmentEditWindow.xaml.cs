using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Учет.Data.Interfaces;
using Учет.Models;

namespace Учет.Views
{
    public partial class DepartmentEditWindow : Window
    {
        private readonly Department _department;
        private readonly bool _isEdit;

        public DepartmentEditWindow(Department department = null)
        {
            InitializeComponent();
            _department = department ?? new Department();
            _isEdit = department != null && department.Id > 0;

            if (_isEdit)
                TitleText.Text = "✏️ Редактирование отдела";

            LoadDepartments();

            if (_isEdit)
            {
                NameBox.Text = _department.Name;
                if (_department.ParentDepartmentId.HasValue)
                    ParentCombo.SelectedValue = _department.ParentDepartmentId.Value;
            }
        }

        private async void LoadDepartments()
        {
            using var scope = App.ServiceProvider.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var departments = (await uow.Departments.GetAllAsync()).ToList();

            var items = new List<Department> { new Department { Id = 0, Name = "— Нет (корневой отдел) —" } };
            items.AddRange(departments);

            ParentCombo.ItemsSource = items;
            ParentCombo.SelectedValuePath = "Id";
        }

        private void NameBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            NameBox.BorderBrush = System.Windows.Media.Brushes.LightGray;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                NameBox.BorderBrush = System.Windows.Media.Brushes.Red;
                MessageBox.Show("Введите название отдела!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _department.Name = NameBox.Text.Trim();

            if (ParentCombo.SelectedValue != null && int.TryParse(ParentCombo.SelectedValue.ToString(), out int parentId) && parentId > 0)
                _department.ParentDepartmentId = parentId;
            else
                _department.ParentDepartmentId = null;

            using var scope = App.ServiceProvider.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            if (_isEdit)
                await uow.Departments.UpdateAsync(_department);
            else
                await uow.Departments.AddAsync(_department);

            await uow.CompleteAsync();

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}