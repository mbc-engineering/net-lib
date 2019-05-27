using System;
using System.Text;

namespace Mbc.TclInterpreter
{
    public class StringInputChannel : AbstractTclChannel
    {
        private readonly StringBuilder _data;

        public StringInputChannel(string data)
        {
            _data = new StringBuilder(data);
        }

        protected override void Flush()
        {
            throw new NotImplementedException();
        }

        protected override void Close()
        {
        }

        protected override int Write(string data)
        {
            throw new NotImplementedException();
        }

        protected override string Read(int len)
        {
            var actlen = Math.Min(len, _data.Length);
            var data = _data.ToString(0, actlen);
            _data.Remove(0, actlen);
            return data;
        }
    }
}
