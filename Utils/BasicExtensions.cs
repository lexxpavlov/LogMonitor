using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LogMonitor.Utils
{
    public static class BasicExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            // ReSharper disable PossibleMultipleEnumeration
            foreach (var item in list)
            {
                action(item);
            }
            return list;
            // ReSharper restore PossibleMultipleEnumeration
        }

        public static ObservableCollection<T> AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> list)
        {
            foreach (var item in list)
            {
                collection.Add(item);
            }
            return collection;
        }
    }
}
