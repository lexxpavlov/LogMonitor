using System;
using System.Windows.Markup;

namespace LogMonitor.Converters
{
    public class StringFormat : MarkupExtension
    {
        private readonly string _format;
        private readonly object[] _args;

        #region ctors

        public StringFormat(string format, object arg1)
        {
            _format = format;
            _args = new [] { arg1 };
        }

        public StringFormat(string format, object arg1, object arg2)
        {
            _format = format;
            _args = new[] { arg1, arg2 };
        }

        public StringFormat(string format, object arg1, object arg2, object arg3)
        {
            _format = format;
            _args = new[] { arg1, arg2, arg3 };
        }

        public StringFormat(string format, object arg1, object arg2, object arg3, object arg4)
        {
            _format = format;
            _args = new[] { arg1, arg2, arg3, arg4 };
        }

        #endregion

        #region MarkupExtension

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return string.Format(_format, _args);
        }

        #endregion
    }
}
