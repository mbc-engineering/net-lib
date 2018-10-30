using System;
using System.Collections.Generic;
using HDF.PInvoke;

namespace Mbc.Hdf5Utils
{
    public class H5Type
    {
        public static readonly H5Type DoubleType = new H5Type(H5T.NATIVE_DOUBLE);
        public static readonly H5Type FloatType = new H5Type(H5T.NATIVE_FLOAT);
        public static readonly H5Type SByteType = new H5Type(H5T.NATIVE_SCHAR);
        public static readonly H5Type ShortType = new H5Type(H5T.NATIVE_SHORT);
        public static readonly H5Type IntType = new H5Type(H5T.NATIVE_INT);
        public static readonly H5Type LongType = new H5Type(H5T.NATIVE_LONG);
        public static readonly H5Type ByteType = new H5Type(H5T.NATIVE_CHAR);
        public static readonly H5Type UShortType = new H5Type(H5T.NATIVE_USHORT);
        public static readonly H5Type UIntType = new H5Type(H5T.NATIVE_UINT);
        public static readonly H5Type ULongType = new H5Type(H5T.NATIVE_ULONG);

        private static Dictionary<Type, H5Type> _nativeTypes = new Dictionary<Type, H5Type>
        {
            [typeof(double)] = DoubleType,
            [typeof(float)] = FloatType,
            [typeof(sbyte)] = SByteType,
            [typeof(short)] = ShortType,
            [typeof(int)] = IntType,
            [typeof(long)] = LongType,
            [typeof(byte)] = ByteType,
            [typeof(ushort)] = UShortType,
            [typeof(uint)] = UIntType,
            [typeof(ulong)] = ULongType,
        };

        private readonly long _typeId;

        public static H5Type NativeToH5(Type type)
        {
            if (!_nativeTypes.ContainsKey(type))
                throw new ArgumentException($"Invalid type: {type}.", nameof(type));
            return _nativeTypes[type];
        }

        private H5Type(long typeId)
        {
            _typeId = typeId;
        }

        internal long Id => _typeId;
    }
}
