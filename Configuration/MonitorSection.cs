using System.Configuration;
using LogMonitor.Configuration.Elements;

namespace LogMonitor.Configuration
{
    public class MonitorSection : ConfigurationSection
    {
        [ConfigurationProperty("SavedLogs")]
        public ConfigElementCollection<LogConfigElement> SavedLogs
        {
            get
            {
                return this["SavedLogs"] as ConfigElementCollection<LogConfigElement>;
            }
        }

        [ConfigurationProperty("Localization")]
        public LocalizationElement Localization
        {
            get
            {
                return this["Localization"] as LocalizationElement;
            }
        }

        [ConfigurationProperty("Window")]
        public WindowConfigElement Window
        {
            get
            {
                return this["Window"] as WindowConfigElement;
            }
        }

        [ConfigurationProperty("LevelColors")]
        public ConfigElementCollection<LevelColorElement> LevelColors
        {
            get
            {
                return this["LevelColors"] as ConfigElementCollection<LevelColorElement>;
            }
        }
        
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public ConfigElementMap<ColumnMappingConfigElement> ColumnMappings
        {
            get
            {
                return (ConfigElementMap<ColumnMappingConfigElement>)base[""];
            }
        }

        public static MonitorSection GetSection()
        {
            return ConfigurationManager.GetSection("MonitorSection") as MonitorSection;
        }
    }
}
