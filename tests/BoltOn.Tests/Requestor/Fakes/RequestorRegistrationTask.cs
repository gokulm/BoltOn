using System;
using BoltOn.Bootstrapping;
using BoltOn.Logging;
using BoltOn.Requestor.Interceptors;
using BoltOn.Transaction;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BoltOn.Tests.Requestor.Fakes
{
	public static class RequestorRegistrationTask
	{
		public static void RegisterRequestorFakes(this BoltOnOptions boltOnOptions)
		{
			var appClock = new Mock<IAppClock>();
			var currentDateTime = DateTime.Parse("10/27/2018 12:51:59 PM");
			appClock.Setup(s => s.Now).Returns(currentDateTime);
			boltOnOptions.ServiceCollection.AddTransient((s) => appClock.Object);

			var testInterceptorLogger = new Mock<IAppLogger<TestInterceptor>>();
			testInterceptorLogger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => RequestorTestHelper.LoggerStatements.Add(st));
			boltOnOptions.ServiceCollection.AddTransient((s) => testInterceptorLogger.Object);

			var stopWatchInterceptorLogger = new Mock<IAppLogger<StopwatchInterceptor>>();
			stopWatchInterceptorLogger.Setup(s => s.Debug(It.IsAny<string>()))
									 .Callback<string>(st => RequestorTestHelper.LoggerStatements.Add(st));
			boltOnOptions.ServiceCollection.AddTransient((s) => stopWatchInterceptorLogger.Object);

			var transactionInterceptorLogger = new Mock<IAppLogger<TransactionInterceptor>>();
			transactionInterceptorLogger.Setup(s => s.Debug(It.IsAny<string>()))
									 .Callback<string>(st => RequestorTestHelper.LoggerStatements.Add(st));
			boltOnOptions.ServiceCollection.AddTransient((s) => transactionInterceptorLogger.Object);

			if (RequestorTestHelper.IsClearInterceptors)
				boltOnOptions.RemoveAllInterceptors();

			if (RequestorTestHelper.IsRemoveStopwatchInterceptor)
				boltOnOptions.RemoveInterceptor<StopwatchInterceptor>();

			boltOnOptions.AddInterceptor<TestInterceptor>();
		}
	}
}
