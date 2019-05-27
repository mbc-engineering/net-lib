using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Mbc.TclInterpreter
{
    /// <summary>
    /// P/Invoke to tcl86t.dll.
    ///
    /// API see https://www.tcl-lang.org/man/tcl8.6/TclLib/contents.htm
    /// </summary>
    internal class TclApi
    {
        static TclApi()
        {
            // preload 64 or 32 Bit DLL (https://stackoverflow.com/questions/10852634/using-a-32bit-or-64bit-dll-in-c-sharp-dllimport)
            var path = new Uri(typeof(TclApi).Assembly.CodeBase).LocalPath;
            var folder = Path.GetDirectoryName(path);

            var subFolder = IntPtr.Size == 8 ? @"bin64" : @"bin32";

            LoadLibrary(Path.Combine(folder, subFolder, "tcl86t.dll"));
        }

        internal enum TclResult
        {
            TclOk = 0,
            TclError = 1,
            TclReturn = 2,
            TclBreak = 3,
            TclContinue = 4,
        }

        [Flags]
        internal enum TclFlags
        {
            TclNone = 0,
            TclGlobalOnly = 1,
            TclNamespaceOnly = 2,
            TclAppendValue = 4,
            TclListElement = 8,
            TclTraceReads = 0x10,
            TclTraceWrites = 0x20,
            TclTraceUnsets = 0x40,
            TclTraceDestroyed = 0x80,
            TclInterpDestroyed = 0x100,
            TclLeaveErrMsg = 0x200,
            TclTraceArray = 0x800,
        }

        internal enum TclModeFlag
        {
            TclReadable = 1 << 1,
            TclWritable = 1 << 2,
            TclException = 1 << 3,
        }

        internal enum TclFileFlags
        {
            TclStdin = 1 << 1,
            TclStdout = 1 << 2,
            TclStderr = 1 << 3,
            TclEnforceMode = 1 << 4,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class TclChannelType
        {
            public IntPtr TypeName;
            public int Version;
            public TclDriverCloseProc CloseProc;
            public TclDriverInputProc InputProc;
            public TclDriverOutputProc OutputProc;
            public IntPtr SeekProc = IntPtr.Zero;
            public IntPtr SetOptionProc = IntPtr.Zero;
            public IntPtr GetOptionProc = IntPtr.Zero;
            public TclDriverWatchProc WatchProc;
            public IntPtr GetHandleProc = IntPtr.Zero;
            public IntPtr Close2Proc = IntPtr.Zero;
            public IntPtr BlockModeProc = IntPtr.Zero;
            public TclDriverFlushProc FlushProc;
            public IntPtr HandlerProc = IntPtr.Zero;
            public IntPtr WideSeekProc = IntPtr.Zero;
            public IntPtr ThreadActionProc = IntPtr.Zero;
            public IntPtr TruncateProc = IntPtr.Zero;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int TclDriverCloseProc(IntPtr instanceData, IntPtr interp);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int TclDriverInputProc(
            IntPtr instanceData,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2), In, Out]
            char[] buf,
            int bufsize,
            ref int errorCode);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int TclDriverOutputProc(
            IntPtr instanceData,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
            char[] buf,
            int toWrite,
            ref int errorCode);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void TclDriverWatchProc(IntPtr instanceData, int mask);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int TclDriverFlushProc(IntPtr instanceData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate TclResult TclCommandCallback(
            IntPtr clientData,
            IntPtr interp,
            Int32 objc,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
            IntPtr[] objv);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        /// <summary>
        /// Tcl_CreateInterp creates a new interpreter structure and returns a token for it.
        /// The token is required in calls to most other Tcl procedures, such as Tcl_CreateCommand, Tcl_Eval,
        /// and Tcl_DeleteInterp. The token returned by Tcl_CreateInterp may only be passed to Tcl routines
        /// called from the same thread as the original Tcl_CreateInterp call. It is not safe for multiple
        /// threads to pass the same token to Tcl's routines. The new interpreter is initialized with the
        /// built-in Tcl commands and with standard variables like tcl_platform and env. To bind in additional
        /// commands, call Tcl_CreateCommand, and to create additional variables, call Tcl_SetVar.
        /// </summary>
        /// <returns>A token for the interpreter.</returns>
        /// <seealso cref="https://www.tcl-lang.org/man/tcl8.6/TclLib/CrtInterp.htm"/>
        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Tcl_CreateInterp();

        /// <summary>
        /// Tcl_DeleteInterp marks an interpreter as deleted; the interpreter will eventually be deleted
        /// when all calls to Tcl_Preserve for it have been matched by calls to Tcl_Release. At that time, all
        /// of the resources associated with it, including variables, procedures, and application-specific
        /// command bindings, will be deleted. After Tcl_DeleteInterp returns any attempt to use Tcl_Eval on
        /// the interpreter will fail and return TCL_ERROR. After the call to Tcl_DeleteInterp it is safe to
        /// examine the interpreter's result, query or set the values of variables, define, undefine or retrieve
        /// procedures, and examine the runtime evaluation stack. See below, in the section
        /// <b>INTERPRETERS AND MEMORY MANAGEMENT</b> for details.
        /// </summary>
        /// <param name="interp">Token for interpreter to be destroyed.</param>
        /// <returns>Tcl_InterpDeleted returns nonzero if Tcl_DeleteInterp was called with interp as its argument;
        /// this indicates that the interpreter will eventually be deleted, when the last call to Tcl_Preserve for
        /// it is matched by a call to Tcl_Release. If nonzero is returned, further calls to Tcl_Eval in this
        /// interpreter will return TCL_ERROR.</returns>
        /// <seealso cref="https://www.tcl-lang.org/man/tcl8.6/TclLib/CrtInterp.htm"/>
        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern TclResult Tcl_DeleteInterp(IntPtr interp);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern TclResult Tcl_Eval(IntPtr interp, string skript);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern TclResult Tcl_EvalFile(IntPtr interp, string fileName);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Tcl_GetObjResult(IntPtr interp);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Tcl_GetStringFromObj(IntPtr tclObj, IntPtr length);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern TclResult Tcl_GetIntFromObj(IntPtr interp, IntPtr tclObj, ref int intPtr);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern TclResult Tcl_GetLongFromObj(IntPtr interp, IntPtr tclObj, ref long longPtr);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern TclResult Tcl_GetDoubleFromObj(IntPtr interp, IntPtr tclObj, ref double longPtr);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern TclResult Tcl_GetBooleanFromObj(IntPtr interp, IntPtr tclObj, ref bool boolPtr);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern TclResult Tcl_ListObjGetElements(IntPtr interp, IntPtr listPtr, ref int objc, ref IntPtr objvPtr);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Tcl_ObjSetVar2(IntPtr interp, IntPtr part1Ptr, IntPtr part2Ptr, IntPtr newValuePtr, TclFlags flags);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern String Tcl_GetStringResult(IntPtr interp);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Tcl_SetObjResult(IntPtr interp, IntPtr objPtr);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Tcl_GetReturnOptions(IntPtr interp, TclResult code);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern TclResult Tcl_DictObjGet(IntPtr interp, IntPtr dictPtr, IntPtr keyPtr, out IntPtr valuePtr);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Tcl_NewStringObj(string bytes, int length);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Tcl_NewIntObj(int intValue);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Tcl_NewLongObj(long intValue);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Tcl_NewDoubleObj(double intValue);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Tcl_NewBooleanObj(bool boolValue);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Tcl_NewListObj(int objc, IntPtr[] objv);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Tcl_CreateCommand(IntPtr interp, string cmdName, TclCommandCallback proc, IntPtr clientData, IntPtr deleteProc);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Tcl_CreateObjCommand(IntPtr interp, string cmdName, TclCommandCallback proc, IntPtr clientData, IntPtr deleteProc);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Tcl_CreateChannel(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(TclChannelTypMarshaler))]
            TclChannelType type,
            [MarshalAs(UnmanagedType.LPStr)]
            string chanName,
            IntPtr instanceData,
            TclModeFlag mask);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Tcl_RegisterChannel(IntPtr interp, IntPtr chan);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Tcl_GetChannelName(IntPtr chan);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Tcl_GetChannelNames(IntPtr interp);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Tcl_Write(IntPtr channel, IntPtr s, int len);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Tcl_Flush(IntPtr channel);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Tcl_SetStdChannel(IntPtr channel, TclFileFlags type);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Tcl_GetStdChannel(TclFileFlags type);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern TclResult Tcl_Preserve(IntPtr clientData);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern TclResult Tcl_Release(IntPtr clientData);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Tcl_Alloc(int size);

        [DllImport("tcl86t.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void TclFreeObj(IntPtr objPtr);

        /*
         * Tcl_IncrRefCount und Tcl_DecrRefCount sind nur als Macros verfügbar. Die Funktionalität
         * wird hier nachgebildet.
         */

        internal static void Tcl_IncrRefCount(IntPtr objPtr)
        {
            var refCount = Marshal.ReadInt32(objPtr, 0);
            refCount++;
            Marshal.WriteInt32(objPtr, 0, refCount);
        }

        internal static void Tcl_DecrRefCount(IntPtr objPtr)
        {
            var refCount = Marshal.ReadInt32(objPtr, 0);
            var newVal = refCount--;
            Marshal.WriteInt32(objPtr, 0, refCount);
            if (newVal <= 1)
                TclFreeObj(objPtr);
        }

        private class TclChannelTypMarshaler : ICustomMarshaler
        {
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedParameter.Local
            private static ICustomMarshaler GetInstance(string cookie)
            {
                return new TclChannelTypMarshaler();
            }

            public object MarshalNativeToManaged(IntPtr pNativeData)
            {
                throw new NotImplementedException();
            }

            public void CleanUpManagedData(object managedObj)
            {
                throw new NotImplementedException();
            }

            public IntPtr MarshalManagedToNative(object managedObj)
            {
                var ctptr = Tcl_Alloc(Marshal.SizeOf(typeof(TclChannelType)));
                Marshal.StructureToPtr((TclChannelType)managedObj, ctptr, false);
                return ctptr;
            }

            public void CleanUpNativeData(IntPtr pNativeData)
            {
                // Der Speicher wird von Tcl beim schliessen der letzten Referenz freigegeben
                // Vorraussetzung ist, das der Pointer Tcl_CreateChannel übergeben wird.
            }

            public int GetNativeDataSize()
            {
                return Marshal.SizeOf(typeof(TclChannelType));
            }
        }
    }
}
