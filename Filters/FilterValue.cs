using System;

namespace LogMonitor.Filters
{
    public struct FilterValue<TValue>
        where TValue : struct, IComparable, IConvertible
    {
        public string Type;
        public string Filter;
        public TValue Value;

        public FilterValue(string type, string filter, TValue value)
        {
            Type = type;
            Filter = filter;
            Value = value;
        }

        public override string ToString()
        {
            return Type + "." + Filter;
        }
    }

    public struct FilterValue<TFilter, TValue>
        where TFilter : struct, IComparable, IConvertible
        where TValue : struct, IComparable, IConvertible
    {
        public string Type;
        public TFilter Filter;
        public TValue Value;

        public FilterValue(string type, TFilter filter, TValue value)
        {
            Type = type;
            Filter = filter;
            Value = value;
        }

        public override string ToString()
        {
            return Type + "." + Filter;
        }
    }

    public struct FilterValue<TType, TFilter, TValue>
        where TType : struct, IComparable, IConvertible
        where TFilter : struct, IComparable, IConvertible
        where TValue : struct, IComparable, IConvertible
    {
        public TType Type;
        public TFilter Filter;
        public TValue Value;

        public FilterValue(TType type, TFilter filter, TValue value)
        {
            Type = type;
            Filter = filter;
            Value = value;
        }

        public override string ToString()
        {
            return Type + "." + Filter;
        }
    }
}
