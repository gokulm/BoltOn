using System;
using Hangfire;

namespace BoltOn.Hangfire
{
	public class AppHangfireJobActivator : JobActivator
	{
		private readonly IServiceProvider _serviceProvider;

		public AppHangfireJobActivator(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public override object ActivateJob(Type type)
		{
			return _serviceProvider.GetService(type);
		}
	}
}
