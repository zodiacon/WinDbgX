using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgX.Extensions {
    static class LinqExtensions {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action) {
            foreach (var item in collection)
                action(item);
            return collection;
        }
    }
}
