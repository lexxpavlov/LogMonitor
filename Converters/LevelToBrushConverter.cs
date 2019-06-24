using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using LogMonitor.Configuration;
using LogMonitor.Filters;

namespace LogMonitor.Converters
{
    public class LevelToBrushConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            LevelValues level;
            if (Enum.TryParse(value.ToString(), true, out level))
            {
                return Config.LevelColors[level];
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        #region MarkupExtension
        public LevelToBrushConverter()
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
