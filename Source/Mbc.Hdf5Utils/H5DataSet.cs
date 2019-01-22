using HDF.PInvoke;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Mbc.Hdf5Utils
{
    public class H5DataSet : IDisposable
    {
        private readonly long _dataSetId;
        private readonly bool[] _growing;
        private bool _disposed;

        public class CreateOptions
        {
            public ulong[] Chunks { get; set; }

            public uint? DeflateLevel { get; set; }
        }

        public static H5DataSet Create(H5File file, string name, H5Type type, H5DataSpace space, CreateOptions options = null)
        {
            return Create(file.Id, name, type.Id, space.Id, options);
        }

        public static H5DataSet Create(H5Group group, string name, H5Type type, H5DataSpace space, CreateOptions options = null)
        {
            return Create(group.Id, name, type.Id, space.Id, options);
        }

        private static H5DataSet Create(long locId, string name, long typeId, long spaceId, CreateOptions options)
        {
            var rank = H5S.get_simple_extent_ndims(spaceId);
            if (rank < 0)
                throw H5Error.GetExceptionFromHdf5Stack();

            var maxDimensions = new ulong[rank];
            var ret = H5S.get_simple_extent_dims(spaceId, null, maxDimensions);
            if (ret < 0)
                throw H5Error.GetExceptionFromHdf5Stack();

            var growing = maxDimensions.Select(x => x == H5S.UNLIMITED).ToArray();

            if (growing.Contains(true) && options?.Chunks == null)
                throw new ArgumentException("Growing data sets require chunking.", nameof(options));

            using (var propList = new H5Id(H5P.create(H5P.DATASET_CREATE), (id) => H5P.close(id)))
            {
                if (options?.Chunks != null)
                {
                    ret = H5P.set_chunk(propList.Id, options.Chunks.Length, options.Chunks);
                    H5Error.CheckH5Result(ret);
                }

                if (options != null && options.DeflateLevel.HasValue)
                {
                    ret = H5P.set_deflate(propList.Id, options.DeflateLevel.Value);
                    H5Error.CheckH5Result(ret);
                }

                var dataSetId = H5D.create(locId, name, typeId, spaceId, dcpl_id: propList.Id);
                if (dataSetId < 0)
                    throw H5Error.GetExceptionFromHdf5Stack();

                return new H5DataSet(dataSetId, growing);
            }
        }

        private H5DataSet(long dataSetId, bool[] growing)
        {
            _dataSetId = dataSetId;
            _growing = growing;
        }

        ~H5DataSet()
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

            var ret = H5D.close(_dataSetId);
            if (disposing)
                H5Error.CheckH5Result(ret);
        }

        internal long Id => _dataSetId;

        public bool IsGrowing => _growing.Contains(true);

        public void ExtendDimensions(ulong[] newDimensions)
        {
            var ret = H5D.set_extent(_dataSetId, newDimensions);
            H5Error.CheckH5Result(ret);
        }

        public H5DataSpace GetSpace()
        {
            var spaceId = H5D.get_space(_dataSetId);
            if (spaceId < 0)
                throw H5Error.GetExceptionFromHdf5Stack();

            return new H5DataSpace(spaceId);
        }

        public void Write<T>(T[,] data, H5DataSpace fileDataSpace = null)
        {
            var memoryType = H5Type.NativeToH5(typeof(T));
            using (var memorySpace = H5DataSpace.CreateSimpleFixed(new[] { (ulong)data.GetLength(0), (ulong)data.GetLength(1) }))
            {
                if (IsGrowing && fileDataSpace == null)
                {
                    ulong[] start;
                    using (var space = GetSpace())
                    {
                        var cur = space.CurrentDimensions;
                        start = new ulong[cur.Length];
                        var end = new ulong[cur.Length];
                        for (var i = 0; i < cur.Length; i++)
                        {
                            if (_growing[i])
                            {
                                start[i] = cur[i];
                                end[i] = cur[i] + (ulong)data.GetLength(i);
                            }
                            else
                            {
                                start[i] = 0;
                                end[i] = cur[i];
                            }
                        }

                        ExtendDimensions(end);
                    }

                    using (var space = GetSpace())
                    {
                        space.Select(start, new[] { (ulong)data.GetLength(0), (ulong)data.GetLength(1) });
                        Write(memorySpace, memoryType, space, data);
                    }
                }
                else
                {
                    Write(memorySpace, memoryType, fileDataSpace, data);
                }
            }
        }

        public void Write<T>(T[] data, H5DataSpace fileDataSpace = null)
        {
            var memoryType = H5Type.NativeToH5(typeof(T));
            using (var memorySpace = H5DataSpace.CreateSimpleFixed(new[] { (ulong)data.Length }))
            {
                if (IsGrowing && fileDataSpace == null)
                {
                    ulong[] start;
                    using (var space = GetSpace())
                    {
                        start = space.CurrentDimensions;
                        var end = new[] { start[0] + (ulong)data.Length };
                        ExtendDimensions(end);
                    }

                    using (var space = GetSpace())
                    {
                        space.Select(start, new[] { (ulong)data.Length });
                        Write(memorySpace, memoryType, space, data);
                    }
                }
                else
                {
                    Write(memorySpace, memoryType, fileDataSpace, data);
                }
            }
        }

        public void Append<T>(T data)
        {
            if (!IsGrowing)
                throw new InvalidOperationException("Append can only be used on growing data sets.");

            var memoryType = H5Type.NativeToH5(typeof(T));
            using (var memorySpace = H5DataSpace.CreateSimpleFixed(new[] { 1UL }))
            {
                ulong[] start;
                using (var space = GetSpace())
                {
                    start = space.CurrentDimensions;
                    if (start.Length != 1)
                        throw new InvalidOperationException("Append can only be used on one dimensional data sets.");

                    var end = new[] { start[0] + 1 };
                    ExtendDimensions(end);
                }

                using (var space = GetSpace())
                {
                    space.Select(start, new[] { 1UL });
                    Write(memorySpace, memoryType, space, data);
                }
            }
        }

        /// <summary>
        /// Fügt mehrere Werte auf einmal hinzu. Das DataSet darf keine fixe Grösse
        /// besitzen und darf nur aus einer Dimension bestehen.
        /// </summary>
        /// <typeparam name="T">der Datentyp der zu schreibenden Werte</typeparam>
        public void AppendAll<T>(T[] data)
        {
            AppendAll(typeof(T), data, data.Length);
        }

        public void AppendAll(Type type, Array data, int count)
        {
            /*
             * Implementierungshinweis: Die hinzufügenden Daten werden als Array mit primitiven
             * Datentyp erwartet, da diese dann ohne Kopiervorgang direkt geschrieben werden
             * können, in dem der HDF5-Lib ein unmanaged Pointer auf die Daten übergeben wird.
             */

            if (!IsGrowing)
                throw new InvalidOperationException("Append can only be used on growing data sets.");

            if (data.Length == 0)
                throw new ArgumentException("No data to append.", nameof(data));

            if (data.Length < count)
                throw new ArgumentOutOfRangeException(nameof(count));

            var memoryType = H5Type.NativeToH5(type);
            using (var memorySpace = H5DataSpace.CreateSimpleFixed(new[] { (ulong)count }))
            {
                ulong[] start;
                using (var space = GetSpace())
                {
                    start = space.CurrentDimensions;
                    if (start.Length != 1)
                        throw new InvalidOperationException("Append can only be used on one dimensional data sets.");

                    var end = new[] { start[0] + (ulong)count };
                    ExtendDimensions(end);
                }

                using (var space = GetSpace())
                {
                    space.Select(start, new[] { (ulong)count });
                    Write(memorySpace, memoryType, space, data);
                }
            }
        }

        /// <summary>
        /// Schreibt die übergebenen Daten in den DataSet.
        /// </summary>
        /// <param name="memoryDataSpace">Der DataSpace der übergebenen Daten <paramref name="data"/>.</param>
        /// <param name="memoryDataType">Der Typ der übergebenen Daten <paramref name="data"/>.</param>
        /// <param name="fileDataSpace">Der DataSpace wo die Daten geschrieben werden sollen.</param>
        /// <param name="data">Die zu schreibenden Daten. Es muss sich dabei entweder um
        /// primitive Datentypen oder um 1-Dimensionale Arrays mit primitive Datentypen handeln.</param>
        private void Write(H5DataSpace memoryDataSpace, H5Type memoryDataType, H5DataSpace fileDataSpace, object data)
        {
            GCHandle gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                var ret = H5D.write(_dataSetId, memoryDataType.Id, memoryDataSpace.Id, fileDataSpace != null ? fileDataSpace.Id : H5S.ALL, H5P.DEFAULT, gcHandle.AddrOfPinnedObject());
                H5Error.CheckH5Result(ret);
            }
            finally
            {
                gcHandle.Free();
            }
        }

        public class Builder
        {
            private H5Type _type;
            private string _name;
            private ulong[] _dims;
            private CreateOptions _options = new CreateOptions();

            public Builder()
            {
            }

            public Builder WithType(H5Type type)
            {
                _type = type;
                return this;
            }

            public Builder WithType(Type type)
            {
                _type = H5Type.NativeToH5(type);
                return this;
            }

            public Builder WithType<T>()
            {
                return WithType(typeof(T));
            }

            public Builder WithName(string name)
            {
                _name = name;
                return this;
            }

            public Builder WithDimension(int dim0, params int[] dimx)
            {
                _dims = new ulong[1 + dimx.Length];
                _dims[0] = (ulong)dim0;
                for (var i = 1; i < _dims.Length; i++)
                {
                    _dims[i] = (ulong)dimx[i - 1];
                }

                return this;
            }

            public Builder WithDeflate(int deflateLevel)
            {
                _options.DeflateLevel = (uint)deflateLevel;
                return this;
            }

            public Builder WithChunking(int dim0, params int[] dimx)
            {
                _options.Chunks = new ulong[1 + dimx.Length];
                _options.Chunks[0] = (ulong)dim0;
                for (var i = 1; i < _options.Chunks.Length; i++)
                {
                    _options.Chunks[i] = (ulong)dimx[i - 1];
                }

                return this;
            }

            public H5DataSet Create(H5File file)
            {
                if (_type == null)
                    throw new InvalidOperationException($"Type must be set ({nameof(WithType)}).");
                if (_name == null)
                    throw new InvalidOperationException($"Name must be set ({nameof(WithName)}).");
                if (_dims == null)
                    throw new InvalidOperationException($"At least one dimension must be set ({nameof(WithDimension)}).");
                if (_options.Chunks != null && _options.Chunks.Length != _dims.Length)
                    throw new InvalidOperationException("Dimension size must match chunk size.");

                file.MakeGroups(H5Utils.GetGroups(_name));

                var current = new ulong[_dims.Length];
                var max = new ulong[_dims.Length];
                for (var i = 0; i < _dims.Length; i++)
                {
                    if (_dims[i] == 0)
                    {
                        current[i] = 0;
                        max[i] = H5S.UNLIMITED;
                    }
                    else
                    {
                        current[i] = max[i] = _dims[i];
                    }
                }

                using (var space = H5DataSpace.CreateSimple(current, max))
                {
                    return H5DataSet.Create(file, _name, _type, space, _options);
                }
            }
        }
    }
}
