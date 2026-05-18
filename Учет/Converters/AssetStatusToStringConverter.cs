using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;
using Учет.Enums;

namespace Учет.Converters
{
    public class AssetStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AssetStatus status)
            {
                return status switch
                {
                    AssetStatus.Registered => "Зарегистрирован",
                    AssetStatus.InService => "В эксплуатации",
                    AssetStatus.UnderMaintenance => "На обслуживании",
                    AssetStatus.UnderRepair => "На ремонте",
                    AssetStatus.Expired => "Просрочено",
                    AssetStatus.WrittenOff => "Списано",
                    _ => status.ToString()
                };
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}