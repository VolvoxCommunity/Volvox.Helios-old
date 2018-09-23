using System;
using System.Collections.Generic;
using System.Text;
using Hangfire;

namespace Volvox.Helios.Service.Jobs
{
    public class VolvoxJobActivator : JobActivator
    {
        readonly private IServiceProvider _serviceProvider;

        public VolvoxJobActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override object ActivateJob(Type jobType)
        {
            return _serviceProvider.GetService(jobType);
        }
    }
}
