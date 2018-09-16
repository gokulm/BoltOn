using BoltOn.Logging.NLog;
using NLog;
using Xunit;
using Moq.AutoMock;
using BoltOn.Tests.Common;

namespace BoltOn.Tests.Logging
{
	public class NLogLoggerAdapterTests
    {
		[Fact]
		public void Debug_InjectedLogger_CallsNLogLogger()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<NLogLoggerAdapter<Employee>>();
			var logger = autoMocker.GetMock<ILogger>();
			var logMessage = "test";


			// act
			sut.Debug(logMessage);

			// assert
			logger.Verify(v => v.Debug(logMessage));
		}
    }
}
