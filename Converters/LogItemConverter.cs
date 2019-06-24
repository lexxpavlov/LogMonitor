using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using LogMonitor.Monitoring;

namespace LogMonitor.Converters
{
    public class LogItemConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var logLine = value as LogLine;
            return logLine != null
                ? logLine[(string)parameter]
                : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        #region MarkupExtension

        public LogItemConverter()
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
