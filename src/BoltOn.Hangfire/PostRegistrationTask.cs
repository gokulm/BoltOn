using System;
using BoltOn.Bootstrapping;
using Hangfire;

namespace BoltOn.Hangfire
{
    public class PostRegistrationTask : IPostRegistrationTask
    {
        private readonly IServiceProvider _serviceProvider;
		private readonly IGlobalConfiguration _globalConfiguration;

		public PostRegistrationTask(IServiceProvider serviceProvider,
            IGlobalConfiguration globalConfiguration)
        {
            _serviceProvider = serviceProvider;
			_globalConfiguration = globalConfiguration;
		}

        public void Run()
        {
            _globalConfiguration.UseActivator(new BoltOnHangfireActivator(_serviceProvider));
        }
    }
}
