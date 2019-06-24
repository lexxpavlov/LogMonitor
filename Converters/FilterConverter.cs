using System;
using System.Windows.Data;
using System.Windows.Markup;
using LogMonitor.Filters;

namespace LogMonitor.Converters
{
    class FilterConverter : MarkupExtension, IValueConverter
    {
        #region IValueConverter

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var filter = (string)parameter;
            var type = filter.Split('.')[0];
            switch (type)
            {
                case "Level":
                    return Check((FilterValue<FilterTypes, LevelValues, bool>)value, filter);
                case "Highlight":
                    return Check((FilterValue<FilterTypes, int, bool>)value, filter);
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var str = ((string) parameter).Split('.');
            var type = str[0];
            var val = str[1];
            switch (type)
            {
                case "Level":
                    var level = (LevelValues)Enum.Parse(typeof(LevelValues), val);
                    return new FilterValue<FilterTypes, LevelValues, bool>(FilterTypes.Level, level, (bool)value);
                case "Highlight":
                    return new FilterValue<FilterTypes, int, bool>(FilterTypes.Highlight, int.Parse(val), (bool)value);
            }
            return null;
        }

        #endregion

        #region private

        private object Check<TType, TFilter, TValue>(FilterValue<TType, TFilter, TValue> obj, string parameter)
            where TType : struct, IComparable, IConvertible
            where TFilter : struct, IComparable, IConvertible
            where TValue : struct, IComparable, IConvertible
        {
            return obj.ToString() == parameter
                ? obj.Value
                : Binding.DoNothing;
        }

        #endregion

        #region MarkupExtension

        public FilterConverter()
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
