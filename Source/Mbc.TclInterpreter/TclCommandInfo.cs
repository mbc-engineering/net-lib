using System;

namespace Mbc.TclInterpreter
{
    internal class TclCommandInfo
    {
        private readonly TclApi.TclCommandCallback _commandCallback;
        private readonly TclCommand _cmd;

        public TclCommandInfo(TclCommand cmd)
        {
            _cmd = cmd;
            // Referenz auf Unmanaged-Delegate, damit GC nicht entfernt
            _commandCallback = Execute;
        }

        private TclApi.TclResult Execute(IntPtr clientData, IntPtr interp, Int32 objc, IntPtr[] objv)
        {
            var paraObj = new IntPtr[objc - 1];
            Array.Copy(objv, 1, paraObj, 0, paraObj.Length);

            var ctx = new TclCommandContext(clientData, interp, paraObj);

            try
            {
                _cmd(ctx);
                return (TclApi.TclResult)ctx.Result;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                ctx.SetResult(FormatException(e));
                return TclApi.TclResult.TclError;
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        private string FormatException(Exception e)
        {
            var ex = e;
            var errorStr = string.Empty;
            while (ex != null)
            {
                errorStr = errorStr + " -> " + e.Message;
                ex = ex.InnerException;
            }

            return errorStr;
        }

        internal TclApi.TclCommandCallback CommandCallback
        {
            get { return _commandCallback; }
        }
    }
}
