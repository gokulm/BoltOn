using System;
using BoltOn.Bootstrapping;
using BoltOn.Data.EF;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;
using BoltOn.Overrides.Mediator;
using BoltOn.Tests.UoW;
using BoltOn.UoW;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BoltOn.Tests.Mediator.Fakes
{
	public class MediatorRegistrationTask : IRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			RegisterMediatorFakes(context);
		}

		private static void RegisterMediatorFakes(RegistrationTaskContext context)
		{
			var changeTrackerInterceptor = new Mock<IBoltOnLogger<ChangeTrackerInterceptor>>();
			changeTrackerInterceptor.Setup(s => s.Debug(It.IsAny<string>()))
									 .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			context.Container.AddTransient(s => changeTrackerInterceptor.Object);


			var boltOnClock = new Mock<IBoltOnClock>();
			var currentDateTime = DateTime.Parse("10/27/2018 12:51:59 PM");
			boltOnClock.Setup(s => s.Now).Returns(currentDateTime);
			context.Container.AddTransient((s) => boltOnClock.Object);

			var testInterceptorLogger = new Mock<IBoltOnLogger<TestInterceptor>>();
			testInterceptorLogger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			context.Container.AddTransient((s) => testInterceptorLogger.Object);

			var stopWatchInterceptorLogger = new Mock<IBoltOnLogger<StopwatchInterceptor>>();
			stopWatchInterceptorLogger.Setup(s => s.Debug(It.IsAny<string>()))
									 .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			context.Container.AddTransient((s) => stopWatchInterceptorLogger.Object);

			var customUoWOptionsBuilder = new Mock<IBoltOnLogger<TestCustomUnitOfWorkOptionsBuilder>>();
			customUoWOptionsBuilder.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			context.Container.AddTransient((s) => customUoWOptionsBuilder.Object);

			var uowOptionsBuilderLogger = new Mock<IBoltOnLogger<UnitOfWorkOptionsBuilder>>();
			uowOptionsBuilderLogger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			context.Container.AddTransient((s) => uowOptionsBuilderLogger.Object);

			if (MediatorTestHelper.IsClearInterceptors)
				context.RemoveAllInterceptors();

			if (MediatorTestHelper.IsCustomizeIsolationLevel)
			{
				context.RemoveInterceptor<ChangeTrackerInterceptor>();
				context.AddInterceptor<CustomChangeTrackerInterceptor>();
				context.Container.AddSingleton<IUnitOfWorkOptionsBuilder, TestCustomUnitOfWorkOptionsBuilder>();
				var customChangeTrackerInterceptorLogger = new Mock<IBoltOnLogger<CustomChangeTrackerInterceptor>>();
				customChangeTrackerInterceptorLogger.Setup(s => s.Debug(It.IsAny<string>()))
										 .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
				context.Container.AddTransient((s) => customChangeTrackerInterceptorLogger.Object);
			}

			if (MediatorTestHelper.IsRemoveStopwatchInterceptor)
				context.RemoveInterceptor<StopwatchInterceptor>();

			context.AddInterceptor<TestInterceptor>();

			//var changeTrackerInterceptor = new Mock<IBoltOnLogger<CustomChangeTrackerInterceptor>>();
			//changeTrackerInterceptor.Setup(s => s.Debug(It.IsAny<string>()))
			//						 .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			//context.Container.AddTransient((s) => changeTrackerInterceptor.Object);
		}
	}
}
