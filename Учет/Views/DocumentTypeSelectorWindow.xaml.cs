using System.Collections.Generic;
using System.Windows;
using Учет.Enums;

namespace Учет.Views
{
    public partial class DocumentTypeSelectorWindow : Window
    {
        public DocType SelectedType { get; private set; }

        public DocumentTypeSelectorWindow()
        {
            InitializeComponent();
            LoadTypes();
        }

        private void LoadTypes()
        {
            var types = new List<DocTypeInfo>
            {
                new DocTypeInfo { Type = DocType.TransferAct, DisplayName = "Акт приема-передачи", Description = "Документ о передаче оборудования" },
                new DocTypeInfo { Type = DocType.DamageAct, DisplayName = "Акт о поломке", Description = "Документ о неисправности оборудования" },
                new DocTypeInfo { Type = DocType.WriteOffAct, DisplayName = "Акт о списании", Description = "Документ о списании оборудования" },
                new DocTypeInfo { Type = DocType.PurchaseRequest, DisplayName = "Заявка на закупку", Description = "Запрос на приобретение нового оборудования" }
            };

            TypeList.ItemsSource = types;
            TypeList.SelectedIndex = 0;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            if (TypeList.SelectedItem is DocTypeInfo selected)
            {
                SelectedType = selected.Type;
                DialogResult = true;
                Close();
            }
        }
    }

    public class DocTypeInfo
    {
        public DocType Type { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}