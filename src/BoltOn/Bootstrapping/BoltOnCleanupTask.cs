using BoltOn.Logging;
using BoltOn.Utilities;

namespace BoltOn.Bootstrapping
{
    public class BoltOnCleanupTask : ICleanupTask
    {
		private readonly IBoltOnLogger<BoltOnCleanupTask> _logger;

		public BoltOnCleanupTask(IBoltOnLogger<BoltOnCleanupTask> logger)
		{
			_logger = logger;
		}

		public void Run()
		{
			_logger.Debug($"{nameof(BoltOnCleanupTask)} invoked");
            // todo: dispose bootstrapper
			_logger.Debug("Cleaned up all the modules...");
		}
    }
}
