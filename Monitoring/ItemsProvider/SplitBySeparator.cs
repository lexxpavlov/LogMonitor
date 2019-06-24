using System;
using System.Collections.Generic;

namespace LogMonitor.Monitoring.ItemsProvider
{
    public class SplitBySeparator : IItemsProvider
    {
        private readonly string _separator;
        private readonly int _columnsCount;

        public SplitBySeparator(string separator, int columnsCount)
        {
            _separator = separator;
            _columnsCount = columnsCount;
        }

        public List<string> GetItems(string line)
        {
            var items = new List<string>(_columnsCount);
            int itemIndex = 0, pos;
            do
            {
                pos = line.IndexOf(_separator, itemIndex, StringComparison.CurrentCulture);
                if (pos >= 0)
                {
                    items.Add(line.Substring(itemIndex, pos - itemIndex));
                    itemIndex = pos + _separator.Length;
                }
            } while (pos >= 0);
            items.Add(line.Substring(itemIndex));
            return items;
        }
    }
}
