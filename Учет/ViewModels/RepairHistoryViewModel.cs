using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Учет.Core;
using Учет.Data.Interfaces;
using Учет.Models;

namespace Учет.ViewModels
{
    public class RepairHistoryViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _uow;
        private readonly int _assetId;
        private readonly string _assetName;

        public ObservableCollection<Repair> Repairs { get; } = new();
        public string AssetInfo => $"История ремонтов: {_assetName}";

        public RelayCommand CloseCommand { get; }

        public RepairHistoryViewModel(IUnitOfWork uow, int assetId, string assetName)
        {
            _uow = uow;
            _assetId = assetId;
            _assetName = assetName;

            CloseCommand = new RelayCommand(_ => CloseWindow());
            _ = LoadRepairsAsync();
        }

        private async Task LoadRepairsAsync()
        {
            try
            {
                var all = await _uow.Repairs.GetAllAsync().ConfigureAwait(false);
                var assetRepairs = all.Where(r => r.AssetId == _assetId).OrderByDescending(r => r.StartDate);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Repairs.Clear();
                    foreach (var r in assetRepairs) Repairs.Add(r);
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки истории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private void CloseWindow()
        {
            if (Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive) is Window win)
                win.Close();
        }
    }
}