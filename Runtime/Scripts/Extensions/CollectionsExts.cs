using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace UnityPatterns.Extensions
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public static class CollectionsExts
    {
        #region HashSet Extensions
        
        [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
        public static HashSet<T> AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> range)
        {
            var data = range as T[] ?? range.ToArray();
            if (!data.Any())
            {
                return hashSet;
            }

            foreach (var value in data)
            {
                hashSet.Add(value);
            }
            
            return hashSet;
        }

        #endregion
    }
}
