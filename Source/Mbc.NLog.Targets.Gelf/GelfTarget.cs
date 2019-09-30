using System;
using System.Collections.Generic;
using NLog;
using NLog.Common;
using NLog.Targets;

namespace Mbc.NLog.Targets.Gelf
{
    /// <summary>
    /// Writes log messages as GELF message.
    /// </summary>
    [Target("Gelf")]
    public class GelfTarget : TargetWithContext
    {
        public GelfTarget()
        {
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var logMessage = RenderLogEvent(Layout, logEvent);
            var logProperties = new Dictionary<string, object>();
            logProperties["log_exception"] = logEvent.Exception;
            logProperties["log_loggername"] = logEvent.LoggerName;
            logProperties["log_timestamp"] = logEvent.TimeStamp;
            logProperties["log_level"] = logEvent.Level;

            foreach (var prop in GetContextProperties(logEvent))
            {
                logProperties["log_prop_" + prop.Key] = prop.Value;
            }

            if (IncludeEventProperties)
            {
                foreach (var prop in logEvent.Properties)
                {
                    var key = prop.Key.ToString();
                    if (!string.IsNullOrEmpty(key))
                        logProperties[key] = prop.Value;
                }
            }

            if (IncludeNdlc)
            {
                logProperties["log_ndlc"] = NestedDiagnosticsLogicalContext.GetAllObjects();
            }

            if (IncludeNdc)
            {
                logProperties["log_ndc"] = NestedDiagnosticsContext.GetAllObjects();
            }

            SendLog(logMessage, logProperties);
        }

        public virtual void SendLog(string logMessage, IDictionary<string, object> logProperties)
        {

        }
    }
}
