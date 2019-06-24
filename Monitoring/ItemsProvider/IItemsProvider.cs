using System.Collections.Generic;

namespace LogMonitor.Monitoring.ItemsProvider
{
    public interface IItemsProvider
    {
        List<string> GetItems(string line);
    }
}
