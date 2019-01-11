using FakeItEasy;
using FluentAssertions;
using Mbc.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Mbc.Common.Test.Reflection
{
    public class FastInvokeTest
    {
        [Fact]
        public void UntypeSetPropertyAccess()
        {
            // Arrange
            var mock = new MockObject();
            var setter = FastInvoke.BuildUntypedSetter<MockObject>(typeof(MockObject).GetProperty(nameof(mock.IntProp)));

            // Act
            setter(mock, 42);

            // Assert
            mock.IntProp.Should().Be(42);
        }

        [Fact]
        public void UntypeGetPropertyAccess()
        {
            // Arrange
            var mock = new MockObject { IntProp = 42 };
            var getter = FastInvoke.BuildUntypedGetter<MockObject>(typeof(MockObject).GetProperty(nameof(mock.IntProp)));

            // Act
            var value = getter(mock);

            // Assert
            value.Should().Be(42);
        }

        [Fact]
        public void UntypeSetFieldAccess()
        {
            // Arrange
            var mock = new MockObject();
            var setter = FastInvoke.BuildUntypedSetter<MockObject>(typeof(MockObject).GetField(nameof(mock.IntField)));

            // Act
            setter(mock, 42);

            // Assert
            mock.IntField.Should().Be(42);
        }

        [Fact]
        public void UntypeGetFieldAccess()
        {
            // Arrange
            var mock = new MockObject { IntField = 42 };
            var getter = FastInvoke.BuildUntypedGetter<MockObject>(typeof(MockObject).GetField(nameof(mock.IntField)));

            // Act
            var value = getter(mock);

            // Assert
            value.Should().Be(42);
        }

        private class MockObject
        {
            public int IntProp { get; set; }

            public int IntField;
        }
    }
}
