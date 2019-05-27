using System.Text;

namespace Mbc.TclInterpreter
{
    public class StringBufferOutputChannel : AbstractTclChannel
    {
        private readonly StringBuilder _str = new StringBuilder();

        protected override int Write(string data)
        {
            _str.Append(data);
            return data.Length;
        }

        public string Data
        {
            get { return _str.ToString(); }
        }
    }
}
