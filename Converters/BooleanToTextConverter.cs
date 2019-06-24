using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace LogMonitor.Converters
{
    public class BooleanToTextConverter : MarkupExtension, IValueConverter
    {
        public string IsTrue { get; set; }
        public string IsFalse { get; set; }
        public string IsThreeState { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return IsThreeState;
            }
            return (bool)value ? IsTrue : IsFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var message = (string)value;
            if (message == IsTrue)
            {
                return true;
            }
            if (message == IsFalse)
            {
                return false;
            }
            return null;
        }

        #region MarkupExtension

        public BooleanToTextConverter()
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
