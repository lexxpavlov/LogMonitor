using System.Configuration;

namespace LogMonitor.Configuration.Elements
{
    public class WindowConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("width", IsRequired = true)]
        public int Width
        {
            get
            {
                return (int)this["width"];
            }
        }

        [ConfigurationProperty("height", IsRequired = true)]
        public int Height
        {
            get
            {
                return (int)this["height"];
            }
        }

        [ConfigurationProperty("monitor", IsRequired = false)]
        public int Monitor
        {
            get
            {
                return (int)this["monitor"];
            }
        }

        [ConfigurationProperty("position", IsRequired = false)]
        public WindowConfig.WindowPosition Position
        {
            get
            {
                return (WindowConfig.WindowPosition)this["position"];
            }
        }
    }
}
