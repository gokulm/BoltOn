using Xunit;

namespace BoltOn.Tests.Logging
{
	public class NetStandardLoggerAdapterTests
    {
        [Fact]
        public void Debug_InjectedLogger_CallsNetStandardLogger()
        {
			// arrange
			//var autoMocker = new AutoMocker();
			//var sut = new NetStandardLoggerAdapter<LoggerTest> (new LoggerFactory());
 		//	var logMessage = "test";

			//// act
			//// as all the NetStandard log methods are extensions methods, they cannot be asserted
			//sut.Debug(logMessage);
			//sut.Error(new Exception());
			//sut.Info(logMessage);
			//sut.Warn(logMessage);
        }
    }

	public class LoggerTest
	{
	}
}


