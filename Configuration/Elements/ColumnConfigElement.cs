using System.Configuration;
using LogMonitor.Monitoring;

namespace LogMonitor.Configuration.Elements
{
    public class ColumnConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("header", IsRequired = true, IsKey = true)]
        public string Header
        {
            get
            {
                return this["header"] as string;
            }
            set
            {
                this["header"] = value;
            }
        }

        [ConfigurationProperty("path", IsRequired = false)]
        public string Path
        {
            get
            {
                return this["path"] as string;
            }
            set
            {
                this["path"] = value;
            }
        }

        [ConfigurationProperty("type", IsRequired = false)]
        public ColumnTypes Type
        {
            get
            {
                return (ColumnTypes)this["type"];
            }
            set
            {
                this["type"] = value;
            }
        }
        
        public override string ToString()
        {
            return Header;
        }
    }
}
