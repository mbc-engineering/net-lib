using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mbc.TclInterpreter
{
    /// <summary>
    /// A facade for TCL lists.
    /// </summary>
    public class TclListFacade
    {
        private readonly IntPtr _interp;
        private readonly IntPtr[] _objv;

        public TclListFacade(IntPtr interp, IntPtr listPtr)
        {
            _interp = interp;
            int objc = 0;
            IntPtr objvPtr = IntPtr.Zero;
            if (TclApi.Tcl_ListObjGetElements(_interp, listPtr, ref objc, ref objvPtr) != TclApi.TclResult.TclOk)
            {
                throw new InvalidOperationException("No list value");
            }

            _objv = new IntPtr[objc];
            Marshal.Copy(objvPtr, _objv, 0, objc);
        }

        public int Parametercount
        {
            get { return _objv.Length; }
        }

        public string GetString(int index)
        {
            var tclStrObj = TclApi.Tcl_GetStringFromObj(_objv[index], IntPtr.Zero);
            return Marshal.PtrToStringAnsi(tclStrObj);
        }

        public int GetInt(int index)
        {
            int intValue = 0;
            var res = TclApi.Tcl_GetIntFromObj(_interp, _objv[index], ref intValue);
            if (res == TclApi.TclResult.TclOk)
                return intValue;
            throw new InvalidOperationException("No integer value: " + index);
        }

        public long GetLong(int index)
        {
            long longValue = 0;
            var res = TclApi.Tcl_GetLongFromObj(_interp, _objv[index], ref longValue);
            if (res == TclApi.TclResult.TclOk)
                return longValue;
            throw new InvalidOperationException("No long value: " + index);
        }

        public double GetDouble(int index)
        {
            double doubleValue = 0;
            var res = TclApi.Tcl_GetDoubleFromObj(_interp, _objv[index], ref doubleValue);
            if (res == TclApi.TclResult.TclOk)
                return doubleValue;
            throw new InvalidOperationException("No double value: " + index);
        }

        public bool GetBool(int index)
        {
            bool boolValue = false;
            var res = TclApi.Tcl_GetBooleanFromObj(_interp, _objv[index], ref boolValue);
            if (res == TclApi.TclResult.TclOk)
                return boolValue;
            throw new InvalidOperationException("No bool value: " + index);
        }

        public TclListFacade GetList(int index)
        {
            return new TclListFacade(_interp, _objv[index]);
        }

        public IEnumerable<string> CreateStringEnumerable()
        {
            for (int i = 0; i < _objv.Length; i++)
                yield return GetString(i);
        }
    }
}
