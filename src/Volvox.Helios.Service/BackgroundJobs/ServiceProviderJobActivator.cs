using System;
using System.Collections.Generic;
using System.Text;
using Hangfire;

namespace Volvox.Helios.Service.BackgroundJobs
{
    /// <summary>
    ///     Job Activiator that allows hangfire to create an instance of a class that requires service injections.
    /// </summary>
    public class ServiceProviderJobActivator : JobActivator
    {
        Func<IServiceProvider> _serviceProviderGetter;

        public ServiceProviderJobActivator(Func<IServiceProvider> serviceProviderGetter)
        {
            _serviceProviderGetter = serviceProviderGetter;
        }

        public override object ActivateJob(Type jobType)
        {
            return _serviceProviderGetter().GetService(jobType);
        }
    }
}
