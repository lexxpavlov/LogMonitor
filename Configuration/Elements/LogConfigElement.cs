using System.Configuration;

namespace LogMonitor.Configuration.Elements
{
    public class LogConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("log", IsRequired = true)]
        public string Log
        {
            get
            {
                return this["log"] as string;
            }
        }

        [ConfigurationProperty("mapping", IsRequired = false)]
        public string Mapping
        {
            get
            {
                return this["mapping"] as string;
            }
        }

        [ConfigurationProperty("autostart", IsRequired = false)]
        public bool AutoStart
        {
            get
            {
                return (bool)this["autostart"];
            }
        }

        [ConfigurationProperty("filterLevels", IsRequired = false)]
        public string FilterLevels
        {
            get
            {
                return this["filterLevels"] as string;
            }
        }

        [ConfigurationProperty("filterHighlight", IsRequired = false)]
        public int FilterHighlight
        {
            get
            {
                return (int)this["filterHighlight"];
            }
        }

        [ConfigurationProperty("filterText", IsRequired = false)]
        public string FilterText
        {
            get
            {
                return this["filterText"] as string;
            }
        }

        public override string ToString()
        {
            return Log;
        }
    }
}
