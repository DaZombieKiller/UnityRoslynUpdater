using System.Collections.Generic;

namespace System.Linq
{
    internal static class EnumerableCompatExtensions
    {
        public static IEnumerable<TSource> Reverse<TSource>(this TSource[] source)
        {
            return Enumerable.Reverse<TSource>((IEnumerable<TSource>)source);
        }
    }
}
