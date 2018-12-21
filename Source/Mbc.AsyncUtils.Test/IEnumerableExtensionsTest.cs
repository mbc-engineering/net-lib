using FakeItEasy;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Mbc.AsyncUtils.Test
{
    public class IEnumerableExtensionsTest
    {
        [Fact]
        public async Task ForEachAsyncWaits()
        {
            // Arrange
            var results = new List<int>();
            int i = 0;
            Task DoSomthing(int counter)
            {
                results.Add(counter);
                return Task.CompletedTask;
            }

            IEnumerable<Func<Task>> list = new List<Func<Task>> { () => DoSomthing(i++), () => DoSomthing(i++), () => DoSomthing(i++), () => DoSomthing(i++) };

            // Act
            await list.ForEachAsync(a => a());

            // Assert
            results.Should().HaveCount(4);
            results.Should().Equal(Enumerable.Range(0, 4));
        }
    }
}
