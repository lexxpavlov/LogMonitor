using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace LogMonitor.Converters
{
    public class BooleanToFontWeightConverter : IValueConverter
    {
        /// <summary>
        /// Возвращать результат с инверсией
        /// </summary>
        public bool Inversed { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = (bool)value;
            var inversed = parameter != null ? parameter.ToString() == "Inversed" : Inversed;
            val = inversed ? !val : val;
            switch (val)
            {
                case true:
                    return FontWeights.Bold;

                case false:
                    return FontWeights.Normal;
            }
            throw new InvalidOperationException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
