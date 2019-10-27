using System;
using BoltOn.Bootstrapping;
using BoltOn.Data.EF;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;
using BoltOn.Overrides.Mediator;
using BoltOn.UoW;
using BoltOn.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BoltOn.Tests.Other
{
	public class TestRegistrationTask : IRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			RegisterDataFakes(context);
			RegisterMediatorFakes(context);
		}

		private static void RegisterDataFakes(RegistrationTaskContext context)
		{
			if (MediatorTestHelper.IsSqlServer)
			{
				context.Container.AddDbContext<SchoolDbContext>(options =>
				{
					options.UseSqlServer("Data Source=127.0.0.1;initial catalog=Testing;persist security info=True;User ID=sa;Password=Password1;");
				});
			}
			else
			{
				context.Container.AddDbContext<SchoolDbContext>(options =>
				{
					options.UseInMemoryDatabase("InMemoryDbForTesting");
					options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));

				});

			}
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

			var customUoWOptionsBuilder = new Mock<IBoltOnLogger<CustomUnitOfWorkOptionsBuilder>>();
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
				context.Container.AddSingleton<IUnitOfWorkOptionsBuilder, CustomUnitOfWorkOptionsBuilder>();
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
