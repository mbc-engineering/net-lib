using EnsureThat;
using System;
using System.Collections.Generic;

namespace Mbc.Common.Collection
{
    #pragma warning disable SA1618 
    public static class Range
    {
        /// <summary>
        /// Gibt einen Range mit jedem Wert von T zurück.
        /// </summary>
        public static Range<T> All<T>()
            where T : IComparable<T>
        {
            return new Range<T>(Cut<T>.BelowAll(), Cut<T>.AboveAll());
        }

        /// <summary>
        /// Gibt einen Range zurück mit allen Werten grösser oder gleich <paramref name="lower"/>
        /// und kleiner oder gleich <paramref name="upper"/>.
        /// </summary>
        public static Range<T> Closed<T>(T lower, T upper)
            where T : IComparable<T>
        {
            return new Range<T>(Cut<T>.BelowValue(lower), Cut<T>.AboveValue(upper));
        }

        /// <summary>
        /// Gibt einen Range zurück mit allen Werten grösser <paramref name="lower"/>
        /// und kleiner <paramref name="upper"/>.
        /// </summary>
        public static Range<T> Open<T>(T lower, T upper)
            where T : IComparable<T>
        {
            return new Range<T>(Cut<T>.AboveValue(lower), Cut<T>.BelowValue(upper));
        }

        /// <summary>
        /// Gibt einen Range zurück mit allen Werten grösser <paramref name="lower"/>
        /// und kleiner oder gleich <paramref name="upper"/>.
        /// </summary>
        public static Range<T> OpenClosed<T>(T lower, T upper)
            where T : IComparable<T>
        {
            return new Range<T>(Cut<T>.AboveValue(lower), Cut<T>.AboveValue(upper));
        }

        /// <summary>
        /// Gibt einen Range zurück mit allen Werten grösser oder gleich <paramref name="lower"/>
        /// und kleiner <paramref name="upper"/>.
        /// </summary>
        public static Range<T> ClosedOpen<T>(T lower, T upper)
            where T : IComparable<T>
        {
            return new Range<T>(Cut<T>.BelowValue(lower), Cut<T>.BelowValue(upper));
        }

        /// <summary>
        /// Gibt einen Range zurück mit allen Werten grösser oder gleich <paramref name="endpoint"/>.
        /// </summary>
        public static Range<T> AtLeast<T>(T endpoint)
            where T : IComparable<T>
        {
            return new Range<T>(Cut<T>.BelowValue(endpoint), Cut<T>.AboveAll());
        }

        /// <summary>
        /// Gibt einen Range zurück mit allen Werten grösser <paramref name="endpoint"/>.
        /// </summary>
        public static Range<T> GreaterThan<T>(T endpoint)
            where T : IComparable<T>
        {
            return new Range<T>(Cut<T>.AboveValue(endpoint), Cut<T>.AboveAll());
        }

        /// <summary>
        /// Gibt einen Range zurück mit allen Werten kleiner <paramref name="endpoint"/>.
        /// </summary>
        public static Range<T> AtMost<T>(T endpoint)
            where T : IComparable<T>
        {
            return new Range<T>(Cut<T>.BelowAll(), Cut<T>.AboveValue(endpoint));
        }

        /// <summary>
        /// Gibt einen Range zurück mit allen Werten kleiner oder gleich <paramref name="endpoint"/>.
        /// </summary>
        public static Range<T> LessThan<T>(T endpoint)
            where T : IComparable<T>
        {
            return new Range<T>(Cut<T>.BelowAll(), Cut<T>.BelowValue(endpoint));
        }

        /// <summary>
        /// Gibt einen minimalen Range zurück, der alle übergebenen Werte einschliesst.
        /// </summary>
        public static Range<T> EncloseAll<T>(IEnumerable<T> values)
            where T : IComparable<T>
        {
            Ensure.Any.IsNotNull(values);

            var e = values.GetEnumerator();
            T min = e.Current;
            T max = min;
            while (e.MoveNext())
            {
                var v = e.Current;
                min = v.CompareTo(min) < 0 ? v : min;
                max = v.CompareTo(max) > 0 ? v : max;
            }

            return Closed(min, max);
        }

        /// <summary>
        /// Gibt einen Range zurück, der nur den übergebenen Wert enthält.
        /// </summary>
        public static Range<T> Singleton<T>(T value)
            where T : IComparable<T>
        {
            return Closed(value, value);
        }
    }
}
