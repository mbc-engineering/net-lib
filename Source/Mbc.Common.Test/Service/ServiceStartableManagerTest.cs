using Autofac;
using FluentAssertions;
using Mbc.Common.Interface;
using Mbc.Common.Service;
using System;
using Xunit;

namespace Mbc.Common.Test.Service
{
    public class ServiceStartableManagerTest
    {
        [Fact]
        public void ServiceMustBeSingleton()
        {
            // Arrange
            var iocBuilder = new ContainerBuilder();
            iocBuilder.RegisterType<TestService1>().AsSelf();
            var ioc = iocBuilder.Build();
            var ssm = new ServiceStartableManager(ioc);

            // Act
            var exception = Record.Exception(() => ssm.StartStartableComponents());

            // Assert
            exception.Should().BeOfType<InvalidOperationException>();
            exception.Message.Should().Be("Services with IServiceStartable should be declared as SingleInstance.");
        }

        [Fact]
        public void StartOnlyOnce()
        {
            // Arrange
            var iocBuilder = new ContainerBuilder();
            iocBuilder.RegisterType<TestService1>().AsSelf().SingleInstance();
            var ioc = iocBuilder.Build();
            var ssm = new ServiceStartableManager(ioc);

            // Act
            ssm.StartStartableComponents();
            ssm.StopStartableComponents();
            ssm.StartStartableComponents();

            // Assert
            ssm.StartedServices.Should().HaveCount(0);
        }

        [Fact]
        public void StartAndStop()
        {
            // Arrange
            var iocBuilder = new ContainerBuilder();
            iocBuilder.RegisterType<Service>().AsSelf().SingleInstance();
            iocBuilder.RegisterType<TestService1>().AsSelf().SingleInstance();
            iocBuilder.RegisterType<TestService2>().AsSelf().SingleInstance();
            iocBuilder.RegisterType<TestService3>().AsSelf().SingleInstance();
            var ioc = iocBuilder.Build();
            var ssm = new ServiceStartableManager(ioc);

            // Act 1
            ssm.StartStartableComponents();

            // Assert
            ssm.StartedServices.Should().HaveCount(3);

            // Act 1
            ssm.StopStartableComponents();

            // Assert
            ssm.StartedServices.Should().HaveCount(0);
        }

        #region " Test classes "

        internal abstract class TestServiceBase : IServiceStartable
        {
            public void Start()
            {
            }

            public void Stop()
            {
            }
        }

        internal class Service
        {
        }

        internal class TestService1 : TestServiceBase
        {
        }

        internal class TestService2 : TestServiceBase
        {
        }

        internal class TestService3 : TestServiceBase
        {
        }

        #endregion
    }
}
