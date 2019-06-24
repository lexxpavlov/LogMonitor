using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace LogMonitor.Converters
{
    public class BooleanToIsCheckedConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = (bool?) value;
            return val == true;
        }

        #region MarkupExtension

        public BooleanToIsCheckedConverter()
        {
            
        }

        /// <summary>
        /// Отображать конвертер в списке
        /// </summary>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        #endregion
    }
}
