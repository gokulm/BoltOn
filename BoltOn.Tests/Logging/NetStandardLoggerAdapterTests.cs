using BoltOn.Logging.NetStandard;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq.AutoMock;
using Moq;
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
			var sut = autoMocker.CreateInstance<NetStandardLoggerAdapter<Employee>>();
			var logger = autoMocker.GetMock<ILogger<Employee>>();
			var logMessage = "test";

			// act
			sut.Debug(logMessage);
			//var ex = new Exception(logMessage);
			//sut.Error(ex);

			// assert 
			logger.Verify(v => v.Log(LogLevel.Debug, 0, logMessage, null, It.IsAny<Func<object, Exception, string>>()));
        }
    }

	public class Employee
	{
	}
}


