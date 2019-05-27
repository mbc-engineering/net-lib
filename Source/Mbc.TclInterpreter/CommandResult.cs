namespace Mbc.TclInterpreter
{
    public enum CommandResult
    {
        Ok = TclApi.TclResult.TclOk,
        Error = TclApi.TclResult.TclError,
        Return = TclApi.TclResult.TclReturn,
        Break = TclApi.TclResult.TclBreak,
        Continue = TclApi.TclResult.TclContinue,
    }
}
