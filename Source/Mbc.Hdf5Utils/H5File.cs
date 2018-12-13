using System;
using System.Collections.Generic;
using System.Text;
using HDF.PInvoke;

namespace Mbc.Hdf5Utils
{
    public sealed class H5File : IDisposable
    {
        [Flags]
        public enum Flags : uint
        {
            ReadOnly = H5F.ACC_RDONLY,
            ReadWrite = H5F.ACC_RDWR,
            Overwrite = H5F.ACC_TRUNC,
            CreateOnly = H5F.ACC_EXCL,
            OpenOrCreate = H5F.ACC_CREAT,
        }

        private readonly long _fileId;
        private bool _disposed;

        public H5File(string filename, Flags flags)
        {
            if ((flags & (Flags.CreateOnly | Flags.OpenOrCreate)) != 0)
            {
                _fileId = H5F.create(filename, (uint)flags);
            }
            else
            {
                _fileId = H5F.open(filename, (uint)flags);
            }

            if (_fileId < 0)
                throw H5Error.GetExceptionFromHdf5Stack();
        }

        ~H5File()
        {
            Dispose(false);
        }

        public string GetName()
        {
            var len = H5F.get_name(_fileId, null, IntPtr.Zero).ToInt32();
            H5Error.CheckH5Result(len);

            var name = new StringBuilder(len + 1);
            var ret = H5F.get_name(_fileId, name, new IntPtr(name.Capacity));
            H5Error.CheckH5Result(ret.ToInt32());

            return name.ToString();
        }

        internal long Id => _fileId;

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

            var ret = H5F.close(_fileId);
            if (disposing)
                H5Error.CheckH5Result(ret);
        }

        public void Flush()
        {
            var ret = H5F.flush(_fileId, H5F.scope_t.GLOBAL);
            H5Error.CheckH5Result(ret);
        }

        public H5Group OpenGroup(string name)
        {
            var groupId = H5G.open(_fileId, name);
            if (groupId < 0)
                throw H5Error.GetExceptionFromHdf5Stack();
            return new H5Group(groupId);
        }

        public H5Group CreateGroup(string name)
        {
            H5Group.CreateMissingGroups(_fileId, name);
            return OpenGroup(name);
        }

        public void MakeGroups(string name)
        {
            H5Group.CreateMissingGroups(_fileId, name);
        }

        public IEnumerable<string> GetNames()
        {
            return new H5LinkIterator(_fileId).Names;
        }
    }
}
