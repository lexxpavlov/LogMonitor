using System;
using System.Configuration;

namespace LogMonitor.Configuration.Elements
{
    public class ColumnMappingConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("separator", IsRequired = false)]
        public string Separator
        {
            get
            {
                ValidateArguments();
                return (string)this["separator"];
            }
            set { this["separator"] = value; }
        }

        [ConfigurationProperty("pattern", IsRequired = false)]
        public string Pattern
        {
            get
            {
                ValidateArguments();
                return (string)this["pattern"];
            }
            set { this["pattern"] = value; }
        }

        [ConfigurationProperty("default", IsRequired = false)]
        public bool IsDefault
        {
            get { return (bool)this["default"]; }
            set { this["default"] = value; }
        }

        [ConfigurationProperty("ignoreEmptyLines", IsRequired = false)]
        public bool IgnoreEmptyLines
        {
            get { return (bool)this["ignoreEmptyLines"]; }
            set { this["ignoreEmptyLines"] = value; }
        }

        [ConfigurationProperty("columns", IsDefaultCollection = false)]
        public ConfigElementMap<ColumnConfigElement> Columns
        {
            get
            {
                return (ConfigElementMap<ColumnConfigElement>)base["columns"];
            }
        }

        public override string ToString()
        {
            return Name;
        }

        private void ValidateArguments()
        {
            var isSeparatorEmpty = string.IsNullOrWhiteSpace((string)this["separator"]);
            var isPatternEmpty = string.IsNullOrWhiteSpace((string)this["pattern"]);
            if (isSeparatorEmpty && isPatternEmpty || !isSeparatorEmpty && !isPatternEmpty)
            {
                throw new ArgumentException("Use only one of arguments: Separator, Pattern");
            }
        }
    }
}
