using System;
using System.Collections.Generic;
using System.Text;
using Hangfire;

namespace Volvox.Helios.Service.BackgroundJobs
{
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
