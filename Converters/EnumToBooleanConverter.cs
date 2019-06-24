using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace WinShellApp.Converters
{
    public class EnumToBooleanConverter : MarkupExtension, IValueConverter
    {
        #region IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }

        #endregion

        #region MarkupExtension

        public EnumToBooleanConverter()
        {
            
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        #endregion
    }
}
