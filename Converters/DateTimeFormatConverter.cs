using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using LogMonitor.Monitoring;

namespace LogMonitor.Converters
{
    public class DateTimeFormatConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime datetime;
            var logLine = value as LogLine;
            if (logLine != null)
            {
                datetime = logLine.Time;
                if (datetime == DateTime.MinValue)
                {
                    return logLine.TimeString;
                }
            }
            else if (value is DateTime)
            {
                datetime = (DateTime)value;
            }
            else
            {
                return value.ToString();
            }
            var format = parameter != null ? (string)parameter : "yyyy-MM-dd HH:mm:ss.fff";
            return datetime.ToString(format);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        #region MarkupExtension

        public DateTimeFormatConverter()
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
