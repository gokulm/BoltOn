using System;
using BoltOn.Bootstrapping;
using BoltOn.Logging;
using BoltOn.Mediator;
using BoltOn.Mediator.Middlewares;
using BoltOn.Mediator.UoW;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BoltOn.Tests.Mediator
{
	public class TestMediatorRegistrationTask : IBootstrapperRegistrationTask
    {
        public void Run(RegistrationTaskContext context)
        {
            var boltOnClock = new Mock<IBoltOnClock>();
            var currentDateTime = DateTime.Parse("10/27/2018 12:51:59 PM");
            boltOnClock.Setup(s => s.Now).Returns(currentDateTime);
            context.Container.AddTransient((s) => boltOnClock.Object);

            var testMiddlewareLogger = new Mock<IBoltOnLogger<TestMiddleware>>();
            testMiddlewareLogger.Setup(s => s.Debug(It.IsAny<string>()))
                                .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
            context.Container.AddTransient((s) => testMiddlewareLogger.Object);

            var stopWatchMiddlewareLogger = new Mock<IBoltOnLogger<StopwatchMiddleware>>();
            stopWatchMiddlewareLogger.Setup(s => s.Debug(It.IsAny<string>()))
                                     .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
            context.Container.AddTransient((s) => stopWatchMiddlewareLogger.Object);

            //var efAutoDetectChangesMiddleware = new Mock<IBoltOnLogger<EFAutoDetectChangesDisablingMiddleware>>();
            //efAutoDetectChangesMiddleware.Setup(s => s.Debug(It.IsAny<string>()))
            //                         .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
            //context.Container.AddTransient((s) => efAutoDetectChangesMiddleware.Object);

            var customUoWOptionsBuilder = new Mock<IBoltOnLogger<CustomUnitOfWorkOptionsBuilder>>();
            customUoWOptionsBuilder.Setup(s => s.Debug(It.IsAny<string>()))
                                .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
            context.Container.AddTransient((s) => customUoWOptionsBuilder.Object);

            var uowOptionsBuilderLogger = new Mock<IBoltOnLogger<UnitOfWorkOptionsBuilder>>();
            uowOptionsBuilderLogger.Setup(s => s.Debug(It.IsAny<string>()))
                                .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
            context.Container.AddTransient((s) => uowOptionsBuilderLogger.Object);

            if (MediatorTestHelper.IsClearMiddlewares)
                context.Container.RemoveAllMiddlewares();

            if (MediatorTestHelper.IsCustomizeIsolationLevel)
                context.Container.AddSingleton<IUnitOfWorkOptionsBuilder, CustomUnitOfWorkOptionsBuilder>();

            context.Container.AddMiddleware<TestMiddleware>();
        }
    }
}
