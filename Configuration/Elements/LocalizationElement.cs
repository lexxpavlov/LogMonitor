using System.Configuration;

namespace LogMonitor.Configuration.Elements
{
    public class LocalizationElement : ConfigurationElement
    {
        [ConfigurationProperty("lang", IsRequired = true)]
        public string Language
        {
            get
            {
                return this["lang"] as string;
            }
        }

        public override string ToString()
        {
            return Language;
        }
    }
}
