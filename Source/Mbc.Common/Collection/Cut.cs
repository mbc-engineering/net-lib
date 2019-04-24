using System;

namespace Mbc.Common.Collection
{
    /// <summary>
    /// Hilfsklasse für <see cref="Range{T}"/>. Representiert eine Teile
    /// in zwei Abschnitte, unter oder über einen bestimmten Wert. Ein
    /// Range kann daher mit zwei <see cref="Cut{T}"/> instanzen dargestellt
    /// werden.
    /// </summary>
    /// <typeparam name="T">Der Typ der Grenze.</typeparam>
    internal abstract class Cut<T> : IComparable<Cut<T>>
        where T : IComparable<T>
    {
        internal static Cut<T> BelowAll()
        {
            return new BelowAllImpl();
        }

        internal static Cut<T> AboveAll()
        {
            return new AboveAllImpl();
        }

        internal static Cut<T> AboveValue(T value)
        {
            return new AboveValueImpl(value);
        }

        internal static Cut<T> BelowValue(T value)
        {
            return new BelowValueImpl(value);
        }

        internal abstract BoundType TypeAsLowerBound { get; }

        internal abstract BoundType TypeAsUpperBound { get; }

        internal abstract T EndPoint { get; }

        internal bool IsBelowAll => this is BelowAllImpl;

        internal bool IsAboveAll => this is AboveAllImpl;

        internal abstract string DescribeAsLowerBound { get; }

        internal abstract string DescribeAsUpperBound { get; }

        #region IComparable

        public virtual int CompareTo(Cut<T> other)
        {
            if (other.IsBelowAll)
            {
                return 1;
            }

            if (other.IsAboveAll)
            {
                return -1;
            }

            var result = EndPoint.CompareTo(other.EndPoint);
            if (result != 0)
            {
                return result;
            }

            var thisAV = this is AboveValueImpl;
            var otherAV = other is AboveValueImpl;

            if (thisAV && !otherAV)
            {
                return 1;
            }

            if (!thisAV && otherAV)
            {
                return -1;
            }

            return 0;
        }

        #endregion

        internal abstract bool IsLessThan(T value);

        public override bool Equals(object obj)
        {
            if (obj is Cut<T> otherCut)
            {
                return CompareTo(otherCut) == 0;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return EndPoint.GetHashCode();
        }

        #region BelowAllImpl

        private class BelowAllImpl : Cut<T>
        {
            internal override BoundType TypeAsLowerBound => BoundType.Closed;

            internal override BoundType TypeAsUpperBound => BoundType.Open;

            internal override T EndPoint => throw new InvalidOperationException("Range unbounded on this side.");

            internal override string DescribeAsLowerBound => "(-∞";

            internal override string DescribeAsUpperBound => throw new InvalidOperationException();

            public override int CompareTo(Cut<T> other) => other is BelowAllImpl ? 0 : -1;

            internal override bool IsLessThan(T value) => true;

            public override bool Equals(object obj) => obj is BelowAllImpl;

            public override int GetHashCode() => 431;
        }

        #endregion

        #region AboveAllImpl

        private class AboveAllImpl : Cut<T>
        {
            internal override BoundType TypeAsLowerBound => BoundType.Open;

            internal override BoundType TypeAsUpperBound => BoundType.Closed;

            internal override T EndPoint => throw new InvalidOperationException("Range unbounded on this side.");

            internal override string DescribeAsLowerBound => throw new InvalidOperationException();

            internal override string DescribeAsUpperBound => "∞)";

            public override int CompareTo(Cut<T> other) => other is AboveAllImpl ? 0 : 1;

            internal override bool IsLessThan(T value) => false;

            public override bool Equals(object obj) => obj is AboveAllImpl;

            public override int GetHashCode() => 191;
        }

        #endregion

        #region  AboveValue

        private class AboveValueImpl : Cut<T>
        {
            public AboveValueImpl(T endpoint)
            {
                EndPoint = endpoint;
            }

            internal override BoundType TypeAsLowerBound => BoundType.Open;

            internal override BoundType TypeAsUpperBound => BoundType.Closed;

            internal override T EndPoint { get; }

            internal override string DescribeAsLowerBound => $"({EndPoint}";

            internal override string DescribeAsUpperBound => $"{EndPoint}]";

            internal override bool IsLessThan(T value) => EndPoint.CompareTo(value) < 0;
        }

        #endregion

        #region  BelowValue

        private class BelowValueImpl : Cut<T>
        {
            public BelowValueImpl(T endpoint)
            {
                EndPoint = endpoint;
            }

            internal override BoundType TypeAsLowerBound => BoundType.Closed;

            internal override BoundType TypeAsUpperBound => BoundType.Open;

            internal override T EndPoint { get; }

            internal override string DescribeAsLowerBound => $"[{EndPoint}";

            internal override string DescribeAsUpperBound => $"{EndPoint})";

            internal override bool IsLessThan(T value) => EndPoint.CompareTo(value) <= 0;
        }

        #endregion
    }
}
