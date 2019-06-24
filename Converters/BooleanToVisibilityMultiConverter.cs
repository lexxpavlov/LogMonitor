using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace WinShellApp.Converters
{
    public class BooleanToVisibilityMultiConverter : MarkupExtension, IMultiValueConverter
    {
        /// <summary>
        /// Перечисление типа конвертирования
        /// </summary>
        public enum Types
        {
            AllTrue, 
            AllFalse,
            AnyTrue,
            AnyFalse,
        }

        /// <summary>
        /// Тип конвертирования, по умолчанию - And
        /// </summary>
        public Types Type { get; set; }

        /// <summary>
        /// Cостояние при ложном результате - Hidden или Collapsed (по умолчанию - Collapsed)
        /// </summary>
        public bool Hidden { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result;
            switch (Type)
            {
                case Types.AllTrue:
                    result = values.All(v => (bool)v);
                    break;
                case Types.AllFalse:
                    result = values.All(v => !(bool)v);
                    break;
                case Types.AnyTrue:
                    result = values.Any(v => (bool)v);
                    break;
                case Types.AnyFalse:
                    result = values.Any(v => !(bool)v);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return result
                ? Visibility.Visible
                : Hidden ? Visibility.Hidden : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return new[] { DependencyProperty.UnsetValue };
        }

        #region MarkupExtension

        public BooleanToVisibilityMultiConverter()
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
