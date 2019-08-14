using System;
using System.Linq;
using HDF.PInvoke;

namespace Mbc.Hdf5Utils
{
    public class H5DataSpace : IDisposable
    {
        public const ulong UNLIMITED = H5S.UNLIMITED;

        private readonly long _dataSpaceId;
        private bool _disposed;

        public static H5DataSpace CreateSimpleFixed(ulong[] dimension)
        {
            return CreateSimple(dimension, dimension);
        }

        public static H5DataSpace CreateSimple(ulong[] dimension, ulong[] maxDimension = null)
        {
            if (maxDimension != null && dimension.Length != maxDimension.Length)
                throw new ArgumentException("max must have the same length as current", nameof(maxDimension));

            var rank = dimension.Length;

            if (maxDimension == null)
            {
                maxDimension = Enumerable.Repeat(UNLIMITED, rank).ToArray();
            }

            var dataSpaceId = H5S.create_simple(rank, dimension, maxDimension);
            if (dataSpaceId < 0)
                throw H5Error.GetExceptionFromHdf5Stack();

            return new H5DataSpace(dataSpaceId);
        }

        public static HyperslabSelectionBuilder CreateSelectionBuilder() => new HyperslabSelectionBuilder();

        internal H5DataSpace(long dataSpaceId)
        {
            _dataSpaceId = dataSpaceId;
        }

        ~H5DataSpace()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;

            var ret = H5S.close(_dataSpaceId);
            if (disposing)
                H5Error.CheckH5Result(ret);
        }

        internal long Id => _dataSpaceId;

        public int Rank
        {
            get
            {
                var rank = H5S.get_simple_extent_ndims(_dataSpaceId);
                if (rank < 0)
                    throw H5Error.GetExceptionFromHdf5Stack();

                return rank;
            }
        }

        public ulong[] CurrentDimensions
        {
            get
            {
                var rank = Rank;
                var dims = new ulong[rank];

                var ret = H5S.get_simple_extent_dims(_dataSpaceId, dims, null);
                H5Error.CheckH5Result(ret);
                return dims;
            }
        }

        public HyperslabSelector Select(ulong[] start, ulong[] count, ulong[] stride = null, ulong[] block = null)
        {
            return new HyperslabSelector(_dataSpaceId, start, stride, count, block);
        }

        public class HyperslabSelector
        {
            private readonly long _dataSetId;

            internal HyperslabSelector(long dataSetId, ulong[] start, ulong[] stride, ulong[] count, ulong[] block)
            {
                _dataSetId = dataSetId;
                AddSelect(H5S.seloper_t.SET, start, stride, count, block);
            }

            private void AddSelect(H5S.seloper_t op, ulong[] start, ulong[] stride, ulong[] count, ulong[] block)
            {
                var ret = H5S.select_hyperslab(_dataSetId, op, start, stride, count, block);
                H5Error.CheckH5Result(ret);
            }

            public HyperslabSelector Or(ulong[] start, ulong[] count, ulong[] stride = null, ulong[] block = null)
            {
                AddSelect(H5S.seloper_t.OR, start, stride, count, block);
                return this;
            }

            public HyperslabSelector And(ulong[] start, ulong[] count, ulong[] stride = null, ulong[] block = null)
            {
                AddSelect(H5S.seloper_t.AND, start, stride, count, block);
                return this;
            }
        }

        public class HyperslabSelectionBuilder
        {
            private ulong[] _start;
            private ulong[] _count;

            internal HyperslabSelectionBuilder()
            {
            }

            public HyperslabSelectionBuilder Start(ulong dim0, params ulong[] dimx)
            {
                _start = new ulong[dimx.Length + 1];
                _start[0] = dim0;
                for (var i = 1; i < _start.Length; i++)
                {
                    _start[i] = dimx[i - 1];
                }

                return this;
            }

            public HyperslabSelectionBuilder Count(ulong dim0, params ulong[] dimx)
            {
                _count = new ulong[dimx.Length + 1];
                _count[0] = dim0;
                for (var i = 1; i < _count.Length; i++)
                {
                    _count[i] = dimx[i - 1];
                }

                return this;
            }

            public void ApplyTo(H5DataSpace dataSpace)
            {
                new HyperslabSelector(dataSpace.Id, _start, null, _count, null);
            }
        }
    }
}
