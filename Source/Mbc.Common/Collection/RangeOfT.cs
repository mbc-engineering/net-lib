using EnsureThat;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mbc.Common.Collection
{
    /* See https://google.github.io/guava/releases/19.0/api/docs/com/google/common/collect/Range.html */

    /// <summary>
    /// Definiert einen Range oder Interval zwischen zwei vergleichbaren
    /// (<see cref="IComparable{T}"/>) Datentypen.
    /// </summary>
    /// <typeparam name="T">Der Typ des Ranges.</typeparam>
#pragma warning disable SA1649
#pragma warning disable CA2225
    public class Range<T>
        where T : IComparable<T>
    {
        public static bool operator ==(Range<T> a, Range<T> b) => a.Equals(b);

        public static bool operator !=(Range<T> a, Range<T> b) => !a.Equals(b);

        public static Range<T> operator &(Range<T> a, Range<T> b) => a.Intersection(b);

        public static Range<T> operator |(Range<T> a, Range<T> b) => a.Span(b);

        private readonly Cut<T> _lowerBound;
        private readonly Cut<T> _upperBound;

        internal Range(Cut<T> lowerBound, Cut<T> upperBound)
        {
            _lowerBound = Ensure.Any.IsNotNull(lowerBound);
            _upperBound = Ensure.Any.IsNotNull(upperBound);

            if (lowerBound.CompareTo(upperBound) > 0 || lowerBound == Cut<T>.AboveAll() || upperBound == Cut<T>.BelowAll())
            {
                throw new ArgumentException($"Invalid Range: {lowerBound}:{upperBound}");
            }
        }

        /// <summary>
        /// <c>true</c> wenn der Range eine untere Grenze besitzt.
        /// </summary>
        /// <seealso cref="LowerEndpoint"/>
        public bool HasLowerBound => !_lowerBound.IsBelowAll;

        /// <summary>
        /// Liefert die untere Grenze zurück.
        /// </summary>
        public T LowerEndpoint => _lowerBound.EndPoint;

        /// <summary>
        /// Liefert den Typ der unteren Grenze zurück.
        /// </summary>
        public BoundType LowerBoundTyp => _lowerBound.TypeAsLowerBound;

        /// <summary>
        /// <c>true</c> wenn der Range eine obere Grenze besitzt.
        /// </summary>
        public bool HasUpperBound => !_upperBound.IsAboveAll;

        /// <summary>
        /// Liefert die obere Grenze zurück.
        /// </summary>
        public T UpperEndpoint => _upperBound.EndPoint;

        /// <summary>
        /// Liefert den Typ der oberen Grenze zurück.
        /// </summary>
        public BoundType UpperBoundTyp => _upperBound.TypeAsUpperBound;

        /// <summary>
        /// Liefer <c>true</c> zurück, wenn der Range leer ist. Ein leerer
        /// Range ist in der Form [v..v) oder (v..].
        /// </summary>
        public bool IsEmtpy => _lowerBound.Equals(_upperBound);

        /// <summary>
        /// Liefert <c>true</c> zurück, wenn der Range den angegebenen Wert
        /// <paramref name="value"/> enthält.
        /// </summary>
        public bool Contains(T value)
        {
            return _lowerBound.IsLessThan(value) && !_upperBound.IsLessThan(value);
        }

        /// <summary>
        /// Liefert <c>true</c> zurück, wenn der Range alle übergebenen Werte
        /// enthält.
        /// </summary>
        public bool ContainsAll(IEnumerable<T> values)
        {
            if (!values.Any())
            {
                return true;
            }

            /* Hier können optimierungen für spezifische Enumerables
               eingefügt werden (z.B. sortierte Collections) */

            return values.All(x => Contains(x));
        }

        /// <summary>
        /// Liefert <c>true</c> zurück, wenn die Grenzen von <paramref name="other"/>
        /// komplett innerhalb dieses Ranges liegen.
        /// </summary>
        public bool Encloses(Range<T> other)
        {
            return _lowerBound.CompareTo(other._lowerBound) <= 0 && _upperBound.CompareTo(other._upperBound) >= 0;
        }

        /// <summary>
        /// Liefert <c>true</c> zurück, wenn ein (möglicherweise leerer) Range existiert, der
        /// diesen und den <paramref name="other"/> Einschliesst.
        /// </summary>
        public bool IsConnect(Range<T> other)
        {
            return _lowerBound.CompareTo(other._upperBound) <= 0 && other._lowerBound.CompareTo(_upperBound) <= 0;
        }

        /// <summary>
        /// Liefert den Range zurück, der diesen und den übergebenen einschliesst.
        /// </summary>
        /// <exception cref="ArgumentException">Wenn die beiden Ranges nicht verbunden sind.</exception>
        public Range<T> Intersection(Range<T> connectedRange)
        {
            var lower = _lowerBound.CompareTo(connectedRange._lowerBound);
            var upper = _upperBound.CompareTo(connectedRange._upperBound);
            if (lower >= 0 && upper <= 0)
            {
                return this;
            }
            else if (lower <= 0 && upper >= 0)
            {
                return connectedRange;
            }

            return new Range<T>(
                lower >= 0 ? _lowerBound : connectedRange._lowerBound,
                upper <= 0 ? _upperBound : connectedRange._upperBound);
        }

        /// <summary>
        /// Liefert einen minimalen Range zurück, der diesen und den übergebenen Range einschliesst.
        /// </summary>
        public Range<T> Span(Range<T> other)
        {
            var lower = _lowerBound.CompareTo(other._lowerBound);
            var upper = _upperBound.CompareTo(other._upperBound);
            if (lower <= 0 && upper >= 0)
            {
                return this;
            }
            else if (lower >= 0 && upper <= 0)
            {
                return other;
            }

            return new Range<T>(
                lower <= 0 ? _lowerBound : other._lowerBound,
                upper >= 0 ? _upperBound : other._upperBound);
        }

        public override bool Equals(object obj)
        {
            if (obj is Range<T> otherRange)
            {
                return _lowerBound.Equals(otherRange._lowerBound) && _upperBound.Equals(otherRange._upperBound);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _lowerBound.GetHashCode() ^ _upperBound.GetHashCode();
        }

        public override string ToString()
        {
            return $"{_lowerBound.DescribeAsLowerBound}..{_upperBound.DescribeAsUpperBound}";
        }
    }
}
