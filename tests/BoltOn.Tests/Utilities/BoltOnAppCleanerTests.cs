using System;
using System.Collections.Generic;
using BoltOn.Bootstrapping;
using BoltOn.Bus.MassTransit;
using BoltOn.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BoltOn.Tests.Utilities
{
	[Collection("IntegrationTests")]
	public class BoltOnAppCleanerTests : IDisposable
	{
		[Fact]
		public void BoltOnAppCleaner_Clean_ExecutesAllCleanupTasksInOrder()
		{
			// arrange
			BoltOnAppCleanerHelper.LoggerStatements.Clear();
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn(b => b.BoltOnMassTransitBusModule());
            var logger = new Mock<IBoltOnLogger<CleanupTask>>();
            logger.Setup(s => s.Debug(It.IsAny<string>()))
                .Callback<string>(st => BoltOnAppCleanerHelper.LoggerStatements.Add(st));
            serviceCollection.AddTransient(s => logger.Object);


			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();

			// act
			serviceProvider.LoosenBolts();

			// assert
			var testCleanupTask = BoltOnAppCleanerHelper.LoggerStatements.IndexOf($"Executed {nameof(TestCleanupTask)}");
			Assert.True(testCleanupTask != -1, "failed 0");
            var cleanupTask = BoltOnAppCleanerHelper.LoggerStatements.IndexOf("Cleaning up bus...");
            Assert.True(cleanupTask != -1, "failed 1");
            Assert.True(testCleanupTask < cleanupTask, "failed 2");
		}

		public void Dispose()
		{
			BoltOnAppCleanerHelper.LoggerStatements.Clear();
		}
	}

	public class TestCleanupTask : ICleanupTask
	{
		public void Run()
		{
			BoltOnAppCleanerHelper.LoggerStatements.Add($"Executed {nameof(TestCleanupTask)}");
		}
	}

	public static class BoltOnAppCleanerHelper
	{
		public static List<string> LoggerStatements { get; set; } = new List<string>();
	}
}
