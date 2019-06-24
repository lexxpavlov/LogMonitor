using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace LogMonitor.Converters
{
    public class IntToBrushConverter  : MarkupExtension, IValueConverter
    {
        private SolidColorBrush _defaultBrush = Brushes.Transparent;

        public SolidColorBrush DefaultBrush
        {
            get { return _defaultBrush; }
            set { _defaultBrush = value; }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var level = (int)value;
            var brushes = parameter as SolidColorBrush[] ?? new[] { DefaultBrush };

            return level < brushes.Length
                ? brushes[level]
                : brushes[brushes.Length - 1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        #region MarkupExtension

        public IntToBrushConverter()
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
