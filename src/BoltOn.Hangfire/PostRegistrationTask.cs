using System;
using BoltOn.Bootstrapping;
using Hangfire;

namespace BoltOn.Hangfire
{
    public class PostRegistrationTask : IPostRegistrationTask
    {
        private readonly IServiceProvider _serviceProvider;

		public PostRegistrationTask(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
		}

        public void Run()
        {
            GlobalConfiguration.Configuration.UseActivator(new BoltOnHangfireJobActivator(_serviceProvider));
        }
    }
}
