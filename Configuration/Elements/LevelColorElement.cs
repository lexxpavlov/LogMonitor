using System.Configuration;

namespace LogMonitor.Configuration.Elements
{
    public class LevelColorElement : ConfigurationElement
    {
        [ConfigurationProperty("level", IsRequired = true)]
        public string Level
        {
            get
            {
                return this["level"] as string;
            }
        }

        [ConfigurationProperty("color", IsRequired = true)]
        public string Color
        {
            get
            {
                return this["color"] as string;
            }
        }

        public override string ToString()
        {
            return Level + "_" + Color;
        }
    }
}
