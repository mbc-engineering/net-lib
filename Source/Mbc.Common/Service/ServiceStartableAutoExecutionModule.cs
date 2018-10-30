//-----------------------------------------------------------------------------
// Copyright (c) 2018 by mbc engineering GmbH, CH-6015 Luzern
// Licensed under the Apache License, Version 2.0
//-----------------------------------------------------------------------------

using Autofac;
using Autofac.Core;
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
    public class ServiceStartableAutoExecutionModule : Module
    {
        private const string _serviceStartableStartActived = "__ServiceStartableActivated";

        protected override void AttachToComponentRegistration(IComponentRegistry registry, IComponentRegistration registration)
        {
            registration.Activated += OnComponentActivated;
        }

        public static void OnComponentActivated(object sender, ActivatedEventArgs<object> args)
        {
            // nothing we can do if a null event argument is passed (should never happen)
            if (args == null)
            {
                return;
            }

            // check already started
            if (args.Component.Metadata.ContainsKey(_serviceStartableStartActived))
            {
                return;
            }

            // nothing we can do if instance is not a handler
            var startable = args.Instance as IServiceStartable;
            if (startable == null)
            {
                return;
            }

            // execute service start
            startable.Start();

            // Flag as started
            args.Component.Metadata[_serviceStartableStartActived] = true;
        }
    }
}
