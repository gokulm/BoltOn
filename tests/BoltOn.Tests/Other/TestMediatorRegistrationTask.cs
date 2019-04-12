using System;
using BoltOn.Bootstrapping;
using BoltOn.Logging;
using BoltOn.Mediator;
using BoltOn.Mediator.Interceptors;
using BoltOn.UoW;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BoltOn.Tests.Other
{
	public class TestMediatorRegistrationTask : IRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
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

			//var efAutoDetectChangesInterceptor = new Mock<IBoltOnLogger<EFAutoDetectChangesDisablingInterceptor>>();
			//efAutoDetectChangesInterceptor.Setup(s => s.Debug(It.IsAny<string>()))
			//                         .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			//context.Container.AddTransient((s) => efAutoDetectChangesInterceptor.Object);

			var customUoWOptionsBuilder = new Mock<IBoltOnLogger<CustomUnitOfWorkOptionsBuilder>>();
			customUoWOptionsBuilder.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			context.Container.AddTransient((s) => customUoWOptionsBuilder.Object);

			var uowOptionsBuilderLogger = new Mock<IBoltOnLogger<UnitOfWorkOptionsBuilder>>();
			uowOptionsBuilderLogger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			context.Container.AddTransient((s) => uowOptionsBuilderLogger.Object);

			if (MediatorTestHelper.IsClearInterceptors)
				context.Container.RemoveAllInterceptors();

			if (MediatorTestHelper.IsCustomizeIsolationLevel)
				context.Container.AddSingleton<IUnitOfWorkOptionsBuilder, CustomUnitOfWorkOptionsBuilder>();

			context.Container.AddInterceptor<TestInterceptor>();
		}
	}
}
