using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Peppa
{
    public class MainWindowMenuConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string flag = null;
            if (value != null) flag = value.ToString();
            if (flag != "mainWindow")
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
