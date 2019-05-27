using System;

namespace Mbc.TclInterpreter
{
    public abstract class AbstractTclChannel
    {
        /*
         * Delegates brauchen eine Referenz, damit sie der GC
         * nicht aufräumt.
         */
        internal readonly TclApi.TclDriverCloseProc CloseCallback;
        internal readonly TclApi.TclDriverFlushProc FlushCallback;
        internal readonly TclApi.TclDriverInputProc InputCallback;
        internal readonly TclApi.TclDriverOutputProc OutputCallback;
        internal readonly TclApi.TclDriverWatchProc WatchCallback;

        protected AbstractTclChannel()
        {
            CloseCallback = CloseProc;
            FlushCallback = FlushProc;
            InputCallback = InputProc;
            OutputCallback = OutputProc;
            WatchCallback = (data, mask) => { };
        }

        protected virtual void Flush()
        {
        }

        protected virtual void Close()
        {
        }

        protected virtual int Write(string data)
        {
            throw new NotImplementedException();
        }

        protected virtual string Read(int len)
        {
            throw new NotImplementedException();
        }

        private int CloseProc(IntPtr instanceData, IntPtr interp)
        {
            Close();
            return 0; // POSIX error code
        }

        private int FlushProc(IntPtr instanceData)
        {
            Flush();
            return 0;
        }

        private int OutputProc(IntPtr instanceData, char[] buf, int toWrite, ref int errorCode)
        {
            try
            {
                var written = Write(new String(buf));
                return written;
            }
            catch (NotImplementedException)
            {
                errorCode = (int)PosixErrorCodes.ENOSYS;
            }

            return -1;
        }

        private int InputProc(IntPtr instanceData, char[] buf, int bufsize, ref int errorCode)
        {
            try
            {
                var data = Read(bufsize);

                for (var i = 0; i < data.Length; i++)
                    buf[i] = data[i];

                return data.Length;
            }
            catch (NotImplementedException)
            {
                errorCode = (int)PosixErrorCodes.ENOSYS;
            }

            return -1;
        }
    }
}
