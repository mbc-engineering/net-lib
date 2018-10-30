using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mbc.Hdf5Utils
{
    internal class H5Id : IDisposable
    {
        public readonly long _id;
        public readonly Func<long, int> _closer;
        private bool _disposed;

        public H5Id(long id, Func<long, int> closer)
        {
            _id = id;
            _closer = closer;

            if (_id < 0)
                throw H5Error.GetExceptionFromHdf5Stack();
        }

        ~H5Id()
        {
            Dispose(false);
        }

        public long Id => _id;

        public static implicit operator long(H5Id h5Id)
        {
            return h5Id.Id;
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

            var ret = _closer(_id);
            if (disposing)
                H5Error.CheckH5Result(ret);
        }
    }
}
