using System.Windows;
using Учет.ViewModels;

namespace Учет.Views
{
    public partial class RepairHistoryWindow : Window
    {
        public RepairHistoryWindow(RepairHistoryViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}