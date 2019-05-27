using System;
using System.Runtime.InteropServices;

namespace Mbc.TclInterpreter
{
    /// <summary>
    /// The context of a TCL command.
    /// </summary>
    public class TclCommandContext
    {
        private readonly IntPtr _clientData;
        private readonly IntPtr _interp;
        private readonly IntPtr[] _objv;
        private CommandResult _result = CommandResult.Ok;

        public TclCommandContext(IntPtr clientData, IntPtr interp, IntPtr[] objv)
        {
            _clientData = clientData;
            _interp = interp;
            _objv = objv;
        }

        public CommandResult Result
        {
            get { return _result; }
            set { _result = value; }
        }

        public int Parametercount
        {
            get { return _objv.Length; }
        }

        public string GetStringParameter(int index)
        {
            var tclStrObj = TclApi.Tcl_GetStringFromObj(_objv[index], IntPtr.Zero);
            return Marshal.PtrToStringAnsi(tclStrObj);
        }

        public int GetIntParameter(int index)
        {
            int intValue = 0;
            var res = TclApi.Tcl_GetIntFromObj(_interp, _objv[index], ref intValue);
            if (res == TclApi.TclResult.TclOk)
                return intValue;
            throw new InvalidOperationException("No integer value: " + index);
        }

        public long GetLongParameter(int index)
        {
            long longValue = 0;
            var res = TclApi.Tcl_GetLongFromObj(_interp, _objv[index], ref longValue);
            if (res == TclApi.TclResult.TclOk)
                return longValue;
            throw new InvalidOperationException("No long value: " + index);
        }

        public double GetDoubleParameter(int index)
        {
            double doubleValue = 0;
            var res = TclApi.Tcl_GetDoubleFromObj(_interp, _objv[index], ref doubleValue);
            if (res == TclApi.TclResult.TclOk)
                return doubleValue;
            throw new InvalidOperationException("No double value: " + index);
        }

        public bool GetBoolParameter(int index)
        {
            bool boolValue = false;
            var res = TclApi.Tcl_GetBooleanFromObj(_interp, _objv[index], ref boolValue);
            if (res == TclApi.TclResult.TclOk)
                return boolValue;
            throw new InvalidOperationException("No bool value: " + index);
        }

        public TclListFacade GetListParameter(int index)
        {
            return new TclListFacade(_interp, _objv[index]);
        }

        public void SetResult(string val)
        {
            var obj = TclApi.Tcl_NewStringObj(val, -1);
            TclApi.Tcl_SetObjResult(_interp, obj);
        }

        public void SetResult(int val)
        {
            var obj = TclApi.Tcl_NewIntObj(val);
            TclApi.Tcl_SetObjResult(_interp, obj);
        }

        public void SetResult(long val)
        {
            var obj = TclApi.Tcl_NewLongObj(val);
            TclApi.Tcl_SetObjResult(_interp, obj);
        }

        public void SetResult(double val)
        {
            var obj = TclApi.Tcl_NewDoubleObj(val);
            TclApi.Tcl_SetObjResult(_interp, obj);
        }
    }
}
