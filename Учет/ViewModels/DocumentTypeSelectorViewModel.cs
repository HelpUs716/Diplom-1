using System;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Учет.Core;
using Учет.Enums;

namespace Учет.ViewModels
{
    public class DocumentTypeSelectorViewModel : BaseViewModel
    {
        private ObservableCollection<DocTypeInfo> _documentTypes;
        private DocTypeInfo? _selectedType;

        public ObservableCollection<DocTypeInfo> DocumentTypes
        {
            get => _documentTypes;
            set { _documentTypes = value; OnPropertyChanged(); }
        }

        public DocTypeInfo? SelectedType
        {
            get => _selectedType;
            set { _selectedType = value; OnPropertyChanged(); RefreshCommands(); }
        }

        public RelayCommand ConfirmCommand { get; }
        public RelayCommand CancelCommand { get; }

        public DocumentTypeSelectorViewModel()
        {
            LoadDocumentTypes();
            ConfirmCommand = new RelayCommand(_ => OnConfirm(), _ => SelectedType != null);
            CancelCommand = new RelayCommand(_ => OnCancel());
        }

        private void LoadDocumentTypes()
        {
            DocumentTypes = new ObservableCollection<DocTypeInfo>
            {
                new DocTypeInfo { Type = DocType.TransferAct, DisplayName = "Акт приема-передачи", Description = "Документ о передаче оборудования" },
                new DocTypeInfo { Type = DocType.DamageAct, DisplayName = "Акт о поломке", Description = "Документ о неисправности оборудования" },
                new DocTypeInfo { Type = DocType.WriteOffAct, DisplayName = "Акт о списании", Description = "Документ о списании оборудования" },
                new DocTypeInfo { Type = DocType.PurchaseRequest, DisplayName = "Заявка на закупку", Description = "Запрос на приобретение нового оборудования" }
            };

            if (DocumentTypes.Any())
                SelectedType = DocumentTypes[0];
        }

        private void OnConfirm()
        {
            if (Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive) is Window win)
                win.DialogResult = true;
        }

        private void OnCancel()
        {
            if (Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive) is Window win)
                win.DialogResult = false;
        }

        private void RefreshCommands() => CommandManager.InvalidateRequerySuggested();
    }

    public class DocTypeInfo
    {
        public DocType Type { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public override string ToString() => DisplayName;
    }
}