using System.Collections.Generic;

namespace Mbc.Common
{
    /// <summary>
    /// Erweiterungen für Enumerables.
    /// </summary>
    public static class Enumerables
    {
        /// <summary>
        /// Liefert eine <see cref="IEnumerable{T}"/> mit einen einzelnen
        /// Element zurück.
        /// </summary>
        /// <typeparam name="T">zu benutzender typ</typeparam>
        /// <param name="item">object zum zurückgeben</param>
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
    }
}
