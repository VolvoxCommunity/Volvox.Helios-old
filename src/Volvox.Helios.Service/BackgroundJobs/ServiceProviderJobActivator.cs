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
        readonly IServiceProvider _serviceProvider;

        public ServiceProviderJobActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override object ActivateJob(Type jobType)
        {
            return _serviceProvider.GetService(jobType);
        }
    }
}
