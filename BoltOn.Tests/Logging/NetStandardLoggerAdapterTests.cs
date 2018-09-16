using BoltOn.Logging.NetStandard;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq.AutoMock;
using System;

namespace BoltOn.Tests.Logging
{
	public class NetStandardLoggerAdapterTests
    {
        [Fact]
        public void Debug_InjectedLogger_CallsNetStandardLogger()
        {
			// arrange
			var autoMocker = new AutoMocker();
			var sut = new NetStandardLoggerAdapter<LoggerTest> (new LoggerFactory());
 			var logMessage = "test";

			// act
			sut.Debug(logMessage);
			sut.Error(new Exception());
			sut.Info(logMessage);
			sut.Warn(logMessage);
        }
    }

	public class LoggerTest
	{
	}
}


