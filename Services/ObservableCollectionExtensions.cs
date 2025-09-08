// Utils/ObservableCollectionExtensions.cs
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ynost.Utils
{
    internal static class ObservableCollectionExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> target,
                                       IEnumerable<T> items)
        {
            foreach (var i in items)
                target.Add(i);
        }
    }
}
