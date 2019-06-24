using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogMonitor.Monitoring.ItemsProvider
{
    public class SplitByPattern : IItemsProvider
    {
        private readonly string _pattern;
        private readonly int _columnsCount;

        public SplitByPattern(string pattern, int columnsCount)
        {
            _pattern = pattern;
            _columnsCount = columnsCount;
        }

        public List<string> GetItems(string line)
        {
            var items = new List<string>(_columnsCount);
            var match = Regex.Match(line, _pattern);
            items.AddRange(match.Groups.Cast<Group>().Where(g => g.Index > 0).Select(g => g.Value));
            return items;
        }
    }
}
