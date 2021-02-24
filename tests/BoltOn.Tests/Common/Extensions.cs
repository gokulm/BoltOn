using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace BoltOn.Tests.Common
{
	public static class Extensions
	{
		public static void VerifyDebug<T>(this Mock<ILogger<T>> logger, string logMessage)
		{
			logger.Verify(
				   m => m.Log(
					   LogLevel.Debug,
					   It.IsAny<EventId>(),
					   It.Is<It.IsAnyType>((v, _) => v.ToString().Contains(logMessage)),
					   null,
					   It.IsAny<Func<It.IsAnyType, Exception, string>>()));
		}
	}
}
