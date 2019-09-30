using FakeItEasy;
using NLog;
using NLog.Common;
using NLog.Config;
using System;
using System.Collections.Generic;
using Xunit;

namespace Mbc.NLog.Targets.Gelf.Test
{
    public class GelfTargetTest
    {
        [Fact]
        public void Foo()
        {
            // Arrange
            var target = A.Fake<GelfTarget>();
            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
            var logger = LogManager.GetLogger("logger1");

            // Act
            logger.Info(new ArgumentNullException("arg1"), "message1 {foo} to {bar}", 1, 2);

            LogManager.Configuration = null;

            // Assert
            A.CallTo(() => target.SendLog("message1 1 to 2", A<IDictionary<string, object>>.That.IsEqualTo(new Dictionary<string, object>()))).MustHaveHappenedOnceExactly();
        }
    }
}
