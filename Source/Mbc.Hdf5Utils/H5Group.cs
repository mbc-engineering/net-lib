using System;
using System.Collections.Generic;
using HDF.PInvoke;

namespace Mbc.Hdf5Utils
{
    public class H5Group : IDisposable
    {
        private readonly long _groupId;
        private bool _disposed;

        internal H5Group(long groupId)
        {
            _groupId = groupId;
        }

        ~H5Group()
        {
            Dispose(false);
        }

        internal long Id => _groupId;

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

            var ret = H5G.close(_groupId);
            if (disposing)
                H5Error.CheckH5Result(ret);
        }

        public H5Group OpenGroup(string name)
        {
            var groupId = H5G.open(_groupId, name);
            if (groupId < 0)
                throw H5Error.GetExceptionFromHdf5Stack();
            return new H5Group(groupId);
        }

        public H5Group CreateGroup(string name)
        {
            H5Group.CreateMissingGroups(_groupId, name);
            return OpenGroup(name);
        }

        public IEnumerable<string> GetNames()
        {
            return new H5LinkIterator(_groupId).Names;
        }

        internal static void CreateMissingGroups(long id, string name)
        {
            // Mögliche Namen:
            // / -> file
            // /foo/bar -> file -> group(foo) -> group(bar)
            // foo/bar -> current -> group(foo) -> group(bar)
            // ./foo/bar -> current -> group(foo) -> group(bar)

            // Startpunkt festlegen
            var curName = "";
            var remainingName = name;
            if (remainingName.StartsWith("/"))
            {
                remainingName = remainingName.Substring(1);
            }
            else if (remainingName.StartsWith("."))
            {
                remainingName = remainingName.Substring(1);
                curName = ".";
            }
            else
            {
                curName = ".";
            }

            while (remainingName.Length > 0)
            {
                var idx = remainingName.IndexOf('/');
                if (idx > 0)
                {
                    curName += $"/{remainingName.Substring(0, idx)}";
                    remainingName = remainingName.Substring(idx + 1);
                }
                else
                {
                    curName += $"/{remainingName}";
                    remainingName = string.Empty;
                }

                var ret = H5L.exists(id, curName);
                H5Error.CheckH5Result(ret);
                if (ret == 0)
                {
                    var gid = H5G.create(id, curName);
                    if (gid < 0)
                        throw H5Error.GetExceptionFromHdf5Stack();
                    ret = H5G.close(gid);
                    H5Error.CheckH5Result(ret);
                }
            }
        }
    }
}
