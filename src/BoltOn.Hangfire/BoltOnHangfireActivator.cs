using System;
using Hangfire;

namespace BoltOn.Hangfire
{
	public class BoltOnHangfireActivator : JobActivator
	{
		private readonly IServiceProvider _serviceProvider;

		public BoltOnHangfireActivator(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public override object ActivateJob(Type type)
		{
			return _serviceProvider.GetService(type);
		}
	}
}
