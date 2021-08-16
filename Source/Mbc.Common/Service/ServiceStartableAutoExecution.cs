using Autofac;
using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;
using EnsureThat;
using Mbc.Common.Interface;

namespace Mbc.Common.Service
{
    /// <summary>
    /// When this module is registered by the container builder with <code>builder.RegisterModule<ServiceStartableAutoExecutionModule>();</code>
    /// then it will execute auomatically the <see cref="IServiceStartable.Start"/> Interface.
    /// Given the <see cref="IServiceStartable"/> is implemented by a component.
    /// It will executed as soon as the container is built. Autofac will not call the Start()
    /// method when subsequent instances are resolved.
    /// </summary>
    /// <remarks>
    /// For equivalent "Stop" functionality, implement <see cref="IDisposable" />. Autofac
    /// will always dispose a component before any of its dependencies (except in the presence
    /// of circular dependencies, in which case the components in the cycle are disposed in
    /// reverse-construction order.)
    /// </remarks>
    public static class ServiceStartableAutoExecution
    {
        private const string _serviceStartableStartActived = "__ServiceStartableActivated";

        public static void Load(ContainerBuilder builder)
        {
            Ensure.Any.IsNotNull(builder, nameof(builder));

            builder.ComponentRegistryBuilder.Registered += (sender, args) =>
            {
                args.ComponentRegistration.PipelineBuilding += (sender2, pipeline) =>
                {
                    pipeline.Use(PipelinePhase.Activation, MiddlewareInsertionMode.EndOfPhase, (c, next) =>
                    {
                        next(c);

                        // nothing we can do if null is passed (should never happen)
                        if (c == null)
                        {
                            return;
                        }

                        // check already started
                        if (c.Registration.Metadata.ContainsKey(_serviceStartableStartActived))
                        {
                            return;
                        }

                        // nothing we can do if instance is not a handler
                        var startable = c.Instance as IServiceStartable;
                        if (startable == null)
                        {
                            return;
                        }

                        // execute service start
                        startable.Start();

                        // Flag as started
                        c.Registration.Metadata[_serviceStartableStartActived] = true;
                    });
                };
            };
        }
    }
}
