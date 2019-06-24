using System.Collections.Generic;

namespace LogMonitor.Monitoring
{
    public class ColumnMapping
    {
        public string Name { get; private set; }

        public List<ColumnConfig> Columns { get; private set; }

        public string Separator { get; private set; }

        public string Pattern { get; private set; }

        public bool IsDefault { get; private set; }

        public bool IgnoreEmptyLines { get; private set; }

        public ColumnMapping(string name, List<ColumnConfig> columns, string separator = null, string pattern = null, bool isDefault = false, bool ignoreEmptyLines = true)
        {
            Name = name;
            Columns = columns;
            Separator = separator;
            Pattern = pattern;
            IsDefault = isDefault;
            IgnoreEmptyLines = ignoreEmptyLines;
        }
    }
}
