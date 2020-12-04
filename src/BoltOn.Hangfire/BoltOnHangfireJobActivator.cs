using System;
using Hangfire;

namespace BoltOn.Hangfire
{
	public class BoltOnHangfireJobActivator : JobActivator
	{
		private readonly IServiceProvider _serviceProvider;

		public BoltOnHangfireJobActivator(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public override object ActivateJob(Type type)
		{
			return _serviceProvider.GetService(type);
		}
	}
}
