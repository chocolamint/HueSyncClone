using System;
using System.Globalization;
using System.Windows.Data;

namespace HueSyncClone.Core
{
    public class DivideConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (double)value / (double)parameter;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => (double)value * (double)parameter;
    }
}
