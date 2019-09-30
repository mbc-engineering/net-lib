using FakeItEasy;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Xunit;

namespace Mbc.NLog.Targets.Gelf.Test
{
    public class GelfTargetTest
    {
        [Fact]
        public void UseCaseAllFeatures()
        {
            // Arrange
            var target = A.Fake<GelfTarget>(opt => opt.CallsBaseMethods());
            target.Layout = @"${level:uppercase=true}|${logger}|${message}";

            // Aktiviert Structured Logging (Argumente als Properits)
            target.IncludeEventProperties = true;

            /* Entspricht in der Config:
             * <target ...>
             * <contextproperty name="ctx_prop" layout="${threadId}" />
             * </target>
             */
            target.ContextProperties.Add(new TargetPropertyWithContext("ctx_prop", new SimpleLayout("${threadId}")));

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
            var logger = LogManager.GetLogger("logger1");

            // Global Diagnostic Context
            target.IncludeGdc = true;
            GlobalDiagnosticsContext.Set("gdc_ctx", 123);

            // Mapped Diagnostic Context
            target.IncludeMdc = true;
            MappedDiagnosticsContext.Set("mdc_ctx", "foo");

            // Mapped Diagnostic Context
            target.IncludeMdc = true;
            MappedDiagnosticsContext.Set("mdc_ctx", "foo");

            // Mapped Diagnostic Logical Context
            target.IncludeMdlc = true;
            MappedDiagnosticsLogicalContext.Set("mdlc_ctx", "bar");

            // Nested  Context
            target.IncludeNdc = true;
            NestedDiagnosticsContext.Push("N1");
            NestedDiagnosticsContext.Push("N2");

            // Nested Logical Context
            target.IncludeNdlc = true;
            NestedDiagnosticsLogicalContext.Push("L1");
            NestedDiagnosticsLogicalContext.Push("L2");

            // Act
            var exc = new ArgumentNullException("arg1");
            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, null, exc, CultureInfo.CurrentCulture, "message1 {foo} to {bar}: {baz}", new object[] { 1, 2, "xyz" });

            logger.Log(logEvent);

            // Cleanup
            LogManager.Configuration = null;

            // Assert
            var expectedProperties = new Dictionary<string, object>
            {
                ["log_exception"] = exc,
                ["log_loggername"] = "logger1",
                ["log_timestamp"] = logEvent.TimeStamp,
                ["log_level"] = LogLevel.Info,
                ["foo"] = 1,
                ["bar"] = 2,
                ["baz"] = "xyz",
                ["log_prop_ctx_prop"] = Thread.CurrentThread.ManagedThreadId.ToString(),
                ["log_prop_gdc_ctx"] = 123,
                ["log_prop_mdc_ctx"] = "foo",
                ["log_prop_mdlc_ctx"] = "bar",
                ["log_ndlc"] = new object[] { "L2", "L1" },
                ["log_ndc"] = new object[] { "N2", "N1" },
            };
            A.CallTo(() => target.SendLog("INFO|logger1|message1 1 to 2: \"xyz\"", A<IDictionary<string, object>>.That.Matches(x => EqualDict(x, expectedProperties)))).MustHaveHappenedOnceExactly();
        }

        private bool EqualDict(IDictionary<string, object> actual, IDictionary<string, object> expected)
        {
            if (actual.Count != expected.Count)
                return false;

            foreach (var actualItem in actual)
            {
                if (!expected.ContainsKey(actualItem.Key))
                    return false;

                if (actualItem.Value is Array x && expected[actualItem.Key] is Array y)
                {
                    if (x.Length != y.Length)
                        return false;

                    for (int i = 0; i < x.Length; i++)
                    {
                        if (!x.GetValue(i).Equals(y.GetValue(i)))
                            return false;
                    }

                    return true;
                }

                if (!expected[actualItem.Key].Equals(actualItem.Value))
                    return false;
            }

            return true;
        }
    }
}
