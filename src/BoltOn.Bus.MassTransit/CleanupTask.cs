using System;
using BoltOn.Bootstrapping;
using BoltOn.Logger;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bus.MassTransit
{
	public class CleanupTask : ICleanupTask
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly IAppLogger<CleanupTask> _logger;

		public CleanupTask(IServiceProvider serviceProvider,
			IAppLogger<CleanupTask> logger)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
		}

		public void Run()
		{
			_logger.Debug("Cleaning up bus...");
			var busControl = _serviceProvider.GetService<IBusControl>();
			if (busControl == null)
				return;

			_logger.Debug("Stopping bus...");
			busControl.Stop();
			_logger.Debug("Stopped bus");
		}
	}
}
