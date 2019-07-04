using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace Mbc.TclInterpreter
{
    public class TclInterp : IDisposable
    {
        public const string StdIn = "stdin";
        public const string StdOut = "stdout";
        public const string StdErr = "stderr";

        private readonly IntPtr _tclip;
        private readonly Dictionary<string, TclCommandInfo> _commands = new Dictionary<string, TclCommandInfo>();
        private readonly List<AbstractTclChannel> _channels = new List<AbstractTclChannel>();

        public TclInterp()
        {
            _tclip = TclApi.Tcl_CreateInterp();
            if (_tclip == IntPtr.Zero)
                throw new InvalidOperationException("Cannot initialize Tcl interpreteter");
            TclApi.Tcl_Preserve(_tclip);
        }

        ~TclInterp()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            TclApi.Tcl_Release(_tclip);
            TclApi.Tcl_DeleteInterp(_tclip);
        }

        public void RegisterCommand(string cmdName, TclCommand cmd)
        {
            var commandInfo = new TclCommandInfo(cmd);
            _commands[cmdName] = commandInfo;
            TclApi.Tcl_CreateObjCommand(_tclip, cmdName, commandInfo.CommandCallback, IntPtr.Zero, IntPtr.Zero);
        }

        public void SetVariable(string name, string value)
        {
            TclApi.Tcl_ObjSetVar2(_tclip, TclApi.Tcl_NewStringObj(name, -1), IntPtr.Zero, TclApi.Tcl_NewStringObj(value, -1), TclApi.TclFlags.TclGlobalOnly);
        }

        public void SetVariable(string name, int value)
        {
            TclApi.Tcl_ObjSetVar2(_tclip, TclApi.Tcl_NewStringObj(name, -1), IntPtr.Zero, TclApi.Tcl_NewIntObj(value), TclApi.TclFlags.TclGlobalOnly);
        }

        public void SetVariable(string name, long value)
        {
            TclApi.Tcl_ObjSetVar2(_tclip, TclApi.Tcl_NewStringObj(name, -1), IntPtr.Zero, TclApi.Tcl_NewLongObj(value), TclApi.TclFlags.TclGlobalOnly);
        }

        public void SetVariable(string name, double value)
        {
            TclApi.Tcl_ObjSetVar2(_tclip, TclApi.Tcl_NewStringObj(name, -1), IntPtr.Zero, TclApi.Tcl_NewDoubleObj(value), TclApi.TclFlags.TclGlobalOnly);
        }

        public void SetVariable(string name, bool value)
        {
            TclApi.Tcl_ObjSetVar2(_tclip, TclApi.Tcl_NewStringObj(name, -1), IntPtr.Zero, TclApi.Tcl_NewBooleanObj(value), TclApi.TclFlags.TclGlobalOnly);
        }

        public void OpenChannel(string name, AbstractTclChannel channel, ChannelModeFlag flag)
        {
            // Referenz hinzufügen, damit GC das Objekt nicht entfern
            _channels.Add(channel);

            var channelType = new TclApi.TclChannelType()
            {
                Version = 2,
                TypeName = IntPtr.Zero,
                CloseProc = channel.CloseCallback,
                FlushProc = channel.FlushCallback,
                InputProc = channel.InputCallback,
                OutputProc = channel.OutputCallback,
                WatchProc = channel.WatchCallback,
            };

            if (name == StdOut || name == StdErr || name == StdIn)
            {
                TclApi.TclFileFlags type;
                switch (name)
                {
                    case StdIn:
                        type = TclApi.TclFileFlags.TclStdin;
                        break;
                    case StdOut:
                        type = TclApi.TclFileFlags.TclStdout;
                        break;
                    case StdErr:
                        type = TclApi.TclFileFlags.TclStderr;
                        break;
                    default:
                        throw new InvalidEnumArgumentException(name);
                }

                var tclchannel = TclApi.Tcl_CreateChannel(channelType, name, IntPtr.Zero, (TclApi.TclModeFlag)flag);
                TclApi.Tcl_SetStdChannel(tclchannel, type);
                TclApi.Tcl_RegisterChannel(IntPtr.Zero, tclchannel);
            }
            else
            {
                var tclchannel = TclApi.Tcl_CreateChannel(channelType, name, IntPtr.Zero, (TclApi.TclModeFlag)flag);
                TclApi.Tcl_RegisterChannel(_tclip, tclchannel);
            }
        }

        private void SetArgvVariable(string[] args)
        {
            var objv = args.Select(a => TclApi.Tcl_NewStringObj(a, -1)).ToArray();
            var argsList = TclApi.Tcl_NewListObj(objv.Length, objv);
            TclApi.Tcl_ObjSetVar2(_tclip, TclApi.Tcl_NewStringObj("argv", -1), IntPtr.Zero, argsList, TclApi.TclFlags.TclGlobalOnly);
        }

        private string GetStackTrace(TclApi.TclResult result)
        {
            var options = TclApi.Tcl_GetReturnOptions(_tclip, result);

            var key = TclApi.Tcl_NewStringObj("-errorinfo", -1);
            TclApi.Tcl_IncrRefCount(key);
            IntPtr stackTrace;
            TclApi.Tcl_DictObjGet(_tclip, options, key, out stackTrace);
            TclApi.Tcl_DecrRefCount(key);
            string error = null;
            if (stackTrace != IntPtr.Zero)
                error = Marshal.PtrToStringAnsi(TclApi.Tcl_GetStringFromObj(stackTrace, IntPtr.Zero));
            TclApi.Tcl_DecrRefCount(options);
            return error;
        }

        public void EvalScript(string script, params string[] args)
        {
            SetArgvVariable(args);
            var res = TclApi.Tcl_Eval(_tclip, script);
            if (res != TclApi.TclResult.TclOk)
            {
                var error = GetStackTrace(res);
                throw new TclException(error);
            }
        }

        public void EvalFile(string file, params string[] args)
        {
            SetArgvVariable(args);
            var res = TclApi.Tcl_EvalFile(_tclip, file);
            if (res != TclApi.TclResult.TclOk)
            {
                var error = GetStackTrace(res);
                throw new TclException(error);
            }
        }

        public string Result
        {
            get
            {
                var obj = TclApi.Tcl_GetObjResult(_tclip);
                if (obj == IntPtr.Zero)
                    return string.Empty;

                var str = TclApi.Tcl_GetStringFromObj(obj, IntPtr.Zero);
                return Marshal.PtrToStringAnsi(str);
            }
        }
    }
}
