using System;

namespace Mbc.TclInterpreter
{
    public class TclException : Exception
    {
        public TclException(string msg)
            : base(msg)
        {
        }

        public TclException()
        {
        }

        public TclException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
