using System;
using System.Configuration;

namespace LogMonitor.Configuration
{
    public class ConfigElementMap<T> : ConfigurationElementCollection where T : ConfigurationElement, new()
    {
        private string _elementName;

        public T this[int index]
        {
            get
            {
                return (T)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public T this[string name]
        {
            get
            {
                if (IndexOf(name) < 0) return null;
                return (T)BaseGet(name);
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new T();
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return element.ToString();
        }

        public int IndexOf(string name)
        {
            name = name.ToLower();

            for (int idx = 0; idx < base.Count; idx++)
            {
                if (this[idx].ToString().ToLower() == name)
                    return idx;
            }
            return -1;
        }

        public int IndexOf(T element)
        {
            return BaseIndexOf(element);
        }

        public void Add(T element)
        {
            BaseAdd(element);
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }

        public void Remove(T element)
        {
            if (BaseIndexOf(element) >= 0)
                BaseRemove(element.ToString());
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override string ElementName
        {
            get { return _elementName ?? (_elementName = GetElementName()); }
        }

        protected virtual string GetElementName()
        {
            var name = new T().GetType().Name;
            var pos = name.LastIndexOf("ConfigElement", StringComparison.CurrentCulture);
            if (pos == -1) pos = name.LastIndexOf("ConfigurationElement", StringComparison.CurrentCulture);
            if (pos == -1) pos = name.LastIndexOf("Element", StringComparison.CurrentCulture);
            if (pos != -1)
            {
                name = name.Substring(0, pos);
            }
            return name;
        }
    }
}
