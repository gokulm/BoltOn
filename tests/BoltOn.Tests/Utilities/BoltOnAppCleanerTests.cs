using System;
using System.Collections.Generic;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;
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
			serviceCollection.BoltOn();

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();

			// act
			serviceProvider.LoosenBolts();

			// assert
			var testCleanupTask = BoltOnAppCleanerHelper.LoggerStatements.IndexOf($"Executed {nameof(TestCleanupTask)}");
			Assert.True(testCleanupTask != -1, "failed 0");
			// todo: add another cleanup task and check the order of execution
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
