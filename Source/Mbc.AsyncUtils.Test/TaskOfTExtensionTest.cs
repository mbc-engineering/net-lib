using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mbc.AsyncUtils.Test
{
    public class TaskOfTExtensionTest
    {
        [Fact]
        public void TimeoutAfterNormal()
        {
            // Arrange
            var tcs = new TaskCompletionSource<object>();
            var task = tcs.Task;
            var result = new object();

            // Act
            var monitoredTask = task.TimeoutAfter(TimeSpan.FromSeconds(1));

            // Assert
            monitoredTask.Status.Should().Be(TaskStatus.WaitingForActivation);
            tcs.SetResult(result);
            monitoredTask.Status.Should().Be(TaskStatus.RanToCompletion);
            monitoredTask.Result.Should().BeSameAs(result);
        }

        [Fact]
        public void TimeoutAfterException()
        {
            // Arrange
            var tcs = new TaskCompletionSource<object>();
            var task = tcs.Task;
            var ex = new Exception("foo");

            // Act
            var monitoredTask = task.TimeoutAfter(TimeSpan.FromSeconds(1));

            // Assert
            monitoredTask.Status.Should().Be(TaskStatus.WaitingForActivation);
            tcs.SetException(ex);
            monitoredTask.Status.Should().Be(TaskStatus.Faulted);
            monitoredTask.Exception.InnerException.Should().BeSameAs(ex);
        }

        [Fact]
        public void TimeoutAfterCancelled()
        {
            // Arrange
            var tcs = new TaskCompletionSource<object>();
            var task = tcs.Task;

            // Act
            var monitoredTask = task.TimeoutAfter(TimeSpan.FromSeconds(1));

            // Assert
            monitoredTask.Status.Should().Be(TaskStatus.WaitingForActivation);
            tcs.SetCanceled();
            monitoredTask.Status.Should().Be(TaskStatus.Canceled);
        }

        [Fact]
        public void TimeoutAfterTimeout()
        {
            // Arrange
            var tcs = new TaskCompletionSource<object>();
            var task = tcs.Task;

            // Act
            Func<Task<object>> func = async () => await task.TimeoutAfter(TimeSpan.FromSeconds(1));

            // Assert
            func.Should().ThrowAsync<TimeoutException>();
        }

        [Fact]
        public void TimeoutAfterCancelledFromCancellationToken()
        {
            // Arrange
            var tcs = new TaskCompletionSource<object>();
            var task = tcs.Task;
            var cts = new CancellationTokenSource();

            // Act
            var monitoredTask = task.TimeoutAfter(TimeSpan.FromSeconds(1), cts.Token);

            // Assert
            monitoredTask.Status.Should().Be(TaskStatus.WaitingForActivation);
            cts.Cancel();
            try
            {
                monitoredTask.Wait();
            }
            catch (Exception e)
            {
                e.InnerException.Should().BeOfType<TaskCanceledException>();
            }

            monitoredTask.Status.Should().Be(TaskStatus.Canceled);
        }
    }
}
