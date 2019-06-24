using System.Configuration;

namespace LogMonitor.Configuration
{
    public class ConfigElementCollection<T> : ConfigurationElementCollection where T : ConfigurationElement, new()
    {
        public T this[int index]
        {
            get
            {
                return base.BaseGet(index) as T;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new T();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return element.ToString();
        }
    }
}
