using System;
using System.Collections.Generic;
using BoltOn.Bootstrapping;
using BoltOn.Logging;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BoltOn.Tests.Bootstrapping
{
	[Collection("IntegrationTests")]
	public class BoltOnAppCleanerTests : IDisposable
	{
		public BoltOnAppCleanerTests()
		{
			Bootstrapper
				.Instance
				.Dispose();
		}

		//[Fact]
		public void BoltOnAppCleaner_Clean_ExecutesAllCleanupTasksInOrder()
		{
			// arrange
			BoltOnAppCleanerHelper.LoggerStatements.Clear();
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn();

			var logger = new Mock<IBoltOnLogger<BoltOnCleanupTask>>();
			logger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => BoltOnAppCleanerHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();

			// act
			BoltOnAppCleaner.Clean();

			// assert
			var testCleanupTask = BoltOnAppCleanerHelper.LoggerStatements.IndexOf($"Executed {nameof(TestCleanupTask)}");
			Assert.True(testCleanupTask != -1, "failed 0");
			var boltOnCleanupTask = BoltOnAppCleanerHelper.LoggerStatements.IndexOf($"{nameof(BoltOnCleanupTask)} invoked");
			Assert.True(boltOnCleanupTask != -1, "failed 1");
			Assert.True(testCleanupTask < boltOnCleanupTask, "failed 2");
		}

		public void Dispose()
		{
			BoltOnAppCleanerHelper.LoggerStatements.Clear();
			Bootstrapper
				.Instance
				.Dispose();
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
