using System;
using BoltOn.Bootstrapping;
using BoltOn.Logging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bus.MassTransit
{
    public class CleanupTask : ICleanupTask
    {
        private readonly IServiceProvider _serviceProvider;
		private readonly IBoltOnLogger<CleanupTask> _logger;

		public CleanupTask(IServiceProvider serviceProvider, 
			IBoltOnLogger<CleanupTask> boltOnLogger)
        {
            _serviceProvider = serviceProvider;
			_logger = boltOnLogger;
		}

        public void Run()
        {
			_logger.Debug("Cleaning up bus...");
            var busControl = _serviceProvider.GetService<IBusControl>();
			if(busControl != null)
			{
				_logger.Debug("Stopping bus...");
				busControl.Stop();
				_logger.Debug("Stopped bus");
			}
        }
    }
}
