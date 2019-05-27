using FluentAssertions;
using System;
using Xunit;

namespace Mbc.TclInterpreter
{
    public class InterpreterTest : IDisposable
    {
        private TclInterp _intp;

        public InterpreterTest()
        {
            _intp = new TclInterp();
        }

        public void Dispose()
        {
            _intp.Dispose();
        }

        [Fact]
        public void Lifecycle()
        {
            // create/dispose
        }

        [Fact]
        public void SimpleEval()
        {
            _intp.EvalScript("expr 2 + 40");
            _intp.Result.Should().Be("42");
        }

        [Fact]
        public void StringCommand()
        {
            string res = string.Empty;
            TclCommand cmd = ctx =>
            {
                res = ctx.GetStringParameter(0) + " " + ctx.GetStringParameter(1);
            };
            _intp.RegisterCommand("foo", cmd);
            _intp.EvalScript("foo foo called");
            res.Should().Be("foo called");
        }

        [Fact]
        public void IntCommand()
        {
            int res = 0;
            TclCommand cmd = ctx =>
            {
                res = ctx.GetIntParameter(0) + ctx.GetIntParameter(1);
            };
            _intp.RegisterCommand("foo", cmd);
            _intp.EvalScript("foo 1 5");
            res.Should().Be(6);
        }

        [Fact]
        public void ListCommand()
        {
            string res = string.Empty;
            TclCommand cmd = ctx =>
            {
                var list = ctx.GetListParameter(0);
                res = list.GetString(0) + " " + list.GetString(1);
            };
            _intp.RegisterCommand("foo", cmd);
            _intp.EvalScript("foo {foo bar}");
            res.Should().Be("foo bar");
        }

        [Fact]
        public void StringResultCommand()
        {
            TclCommand cmd = ctx =>
            {
                ctx.SetResult("foobar");
            };
            _intp.RegisterCommand("foo", cmd);
            _intp.EvalScript("foo");
            _intp.Result.Should().Be("foobar");
        }

        [Fact]
        public void IntResultCommand()
        {
            TclCommand cmd = ctx =>
            {
                ctx.SetResult(42);
            };
            _intp.RegisterCommand("foo", cmd);
            _intp.EvalScript("foo");
            _intp.Result.Should().Be("42");
        }

        [Fact]
        public void LongResultCommand()
        {
            TclCommand cmd = ctx =>
            {
                ctx.SetResult(42L);
            };
            _intp.RegisterCommand("foo", cmd);
            _intp.EvalScript("foo");
            _intp.Result.Should().Be("42");
        }

        [Fact]
        public void DoubleResultCommand()
        {
            TclCommand cmd = ctx =>
            {
                ctx.SetResult(42.42);
            };
            _intp.RegisterCommand("foo", cmd);
            _intp.EvalScript("foo");
            _intp.Result.Should().Be("42.42");
        }

        [Fact]
        public void StringVariable()
        {
            _intp.SetVariable("foo", "bar");
            _intp.EvalScript("string length $foo");
            _intp.Result.Should().Be("3");
        }

        [Fact]
        public void ConsecutiveEvals()
        {
            _intp.EvalScript("set a 20");
            _intp.EvalScript("set b 22");
            _intp.EvalScript("expr $a + $b");
            _intp.Result.Should().Be("42");
        }

        [Fact]
        public void EvalArguments()
        {
            _intp.EvalScript("expr [lindex $argv 0] + [lindex $argv 1]", "5", "37");
            _intp.Result.Should().Be("42");
        }

        [Fact]
        public void FooChannel()
        {
            var fooCh = new StringBufferOutputChannel();
            _intp.OpenChannel("foo", fooCh, ChannelModeFlag.Writable);
            _intp.EvalScript("puts foo Hello\nflush foo");
            fooCh.Data.Should().Be("Hello\r\n");
        }

        [Fact]
        public void StdoutChannel()
        {
            var stdoutCh = new StringBufferOutputChannel();
            _intp.OpenChannel(TclInterp.StdOut, stdoutCh, ChannelModeFlag.Writable);
            _intp.EvalScript("puts Hello\nflush stdout");
            stdoutCh.Data.Should().Be("Hello\r\n");
        }

        [Fact]
        public void StdoutAndStderrChannel()
        {
            var ch = new StringBufferOutputChannel();
            _intp.OpenChannel(TclInterp.StdOut, ch, ChannelModeFlag.Writable);
            _intp.OpenChannel(TclInterp.StdErr, ch, ChannelModeFlag.Writable);
            _intp.EvalScript("puts Hello\nflush stdout");
            _intp.EvalScript("puts stderr World\nflush stderr");
            ch.Data.Should().Be("Hello\r\nWorld\r\n");
        }

        [Fact]
        public void StdinChannel()
        {
            var stdinCh = new StringInputChannel("Hello");
            _intp.OpenChannel(TclInterp.StdIn, stdinCh, ChannelModeFlag.Readable);
            _intp.EvalScript("read stdin");
            _intp.Result.Should().Be("Hello");
        }

        [Fact]
        public void StdoutNotWritableChannel()
        {
            var stdoutCh = new StringInputChannel(string.Empty);
            _intp.OpenChannel(TclInterp.StdOut, stdoutCh, ChannelModeFlag.Writable);

            Action act = () => _intp.EvalScript("puts Hello\nflush stdout");

            act.Should().Throw<TclException>().WithMessage("error flushing \"stdout\": function not implemented\n    while executing\n\"flush stdout\"");
        }
    }
}
