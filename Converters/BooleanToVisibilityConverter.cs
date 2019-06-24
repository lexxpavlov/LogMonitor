using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace LogMonitor.Converters
{
    public class BooleanToVisibilityConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// Возвращать результат с инверсией
        /// </summary>
        public bool Inversed { get; set; }

        /// <summary>
        /// Cостояние при ложном результате - Hidden или Collapsed (по умолчанию - Collapsed)
        /// </summary>
        public bool Hidden { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = (bool)value;
            var inversed = parameter != null ? parameter.ToString() == "Inversed" : Inversed;
            val = inversed ? !val : val;
            switch (val)
            {
                case true:
                    return Visibility.Visible;

                case false:
                    return Hidden ? Visibility.Hidden : Visibility.Collapsed;
            }
            throw new InvalidOperationException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = (Visibility)value;
            var inversed = parameter != null ? parameter.ToString() == "Inversed" : Inversed;
            var res = !inversed;
            switch (val)
            {
                case Visibility.Visible:
                    return res;

                case Visibility.Hidden:
                case Visibility.Collapsed:
                    return !res;
            }
            throw new InvalidOperationException();
        }

        #region MarkupExtension

        public BooleanToVisibilityConverter()
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
