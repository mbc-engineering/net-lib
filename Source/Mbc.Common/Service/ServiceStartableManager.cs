//-----------------------------------------------------------------------------
// Copyright (c) 2018 by mbc engineering GmbH, CH-6015 Luzern
// Licensed under the Apache License, Version 2.0
//-----------------------------------------------------------------------------

using Autofac;
using Autofac.Core;
using Mbc.Common.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mbc.Common.Service
{
    /// <summary>
    /// This class help to handle the <see cref="IServiceStartable"/> interfaces implemented on services
    /// </summary>
    public class ServiceStartableManager
    {
        private const string _serviceStartableStartActived = "__ServiceStartableActivated";
        private readonly IComponentContext _componentContext;
        private List<IServiceStartable> _startedServices = new List<IServiceStartable>();

        public ServiceStartableManager(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        internal IEnumerable<IServiceStartable> StartedServices => _startedServices;

        /// <summary>
        /// Executes <see cref="IServiceStartable.Start"/> Interface in all components that contians it.
        /// </summary>
        /// <param name="componentContext">The <see cref="IComponentContext" /> in which startables should execute.</param>
        public void StartStartableComponents()
        {
            IComponentRegistry componentRegistry = _componentContext.ComponentRegistry;
            var servicesToStart = componentRegistry.Registrations
                .Where(
                c => typeof(IServiceStartable).IsAssignableFrom(c.Activator.LimitType) && !c.Metadata.ContainsKey(_serviceStartableStartActived));

            Task.WhenAll(
                servicesToStart.Select(
                    r => Task.Factory.StartNew(() => Start(r)))).GetAwaiter().GetResult();
        }

        public void StopStartableComponents()
        {
            Task.WhenAll(
                _startedServices.Select(
                    s => Task.Factory.StartNew(() => s.Stop())
                        .ContinueWith(t =>
                        {
                            if (t.Status == TaskStatus.RanToCompletion)
                            {
                                lock (_startedServices)
                                {
                                    _startedServices.Remove(s);
                                }
                            }
                        }))).GetAwaiter().GetResult();
        }

        private void Start(IComponentRegistration registriaton)
        {
            try
            {
                // Ensure that IServiceStartable services are singletons
                if (registriaton.Sharing != InstanceSharing.Shared)
                {
                    throw new InvalidOperationException($"Services with {nameof(IServiceStartable)} should be declared as SingleInstance.");
                }

                // Execute Start
                var comp = _componentContext.ResolveComponent(registriaton, Enumerable.Empty<Parameter>());
                if (comp is IServiceStartable serviceStartable)
                {
                    serviceStartable.Start();

                    lock (_startedServices)
                    {
                        _startedServices.Add(serviceStartable);
                    }
                }
            }
            finally
            {
                registriaton.Metadata[_serviceStartableStartActived] = true;
            }
        }
    }
}
