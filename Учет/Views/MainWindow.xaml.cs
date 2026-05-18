using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Учет.Models;
using Учет.ViewModels;

namespace Учет.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MainWindowViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;          
            InitializeComponent();
            DataContext = vm;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is Department dept && DataContext is MainWindowViewModel vm)
            {
                vm.SelectedDepartment = dept;
            }
        }
        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element && element.DataContext is Department dept)
            {
                var win = new DepartmentEditWindow(dept);
                win.Owner = this;
                if (win.ShowDialog() == true)
                {
                    ((MainWindowViewModel)DataContext).LoadDepartmentsAsync();
                }
            }
        }
    }
}