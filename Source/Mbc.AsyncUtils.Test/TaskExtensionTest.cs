﻿using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mbc.AsyncUtils.Test
{
    public class TaskExtensionTest
    {
        [Fact]
        public void TimeoutAfterNormal()
        {
            // Arrange
            var tcs = new TaskCompletionSource();
            var task = tcs.Task;

            // Act
            var monitoredTask = task.TimeoutAfter(TimeSpan.FromSeconds(1));

            // Assert
            monitoredTask.Status.Should().Be(TaskStatus.WaitingForActivation);
            tcs.SetResult();
            monitoredTask.Status.Should().Be(TaskStatus.RanToCompletion);
        }

        [Fact]
        public void TimeoutAfterException()
        {
            // Arrange
            var tcs = new TaskCompletionSource();
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
            var tcs = new TaskCompletionSource();
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
            var tcs = new TaskCompletionSource();
            var task = tcs.Task;

            // Act
            Func<Task> func = async () => await task.TimeoutAfter(TimeSpan.FromSeconds(1));

            // Assert
            func.Should().Throw<TimeoutException>();
        }

        [Fact]
        public void TimeoutAfterCancelledFromCancellationToken()
        {
            // Arrange
            var tcs = new TaskCompletionSource();
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

        private class TaskCompletionSource
        {
            private readonly TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>();

            public Task Task => _tcs.Task;

            public void SetResult()
            {
                _tcs.SetResult(null);
            }

            public void SetException(Exception exception)
            {
                _tcs.SetException(exception);
            }

            public void SetCanceled()
            {
                _tcs.SetCanceled();
            }
        }
    }
}
