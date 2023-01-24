using BoltOn.Requestor;
using BoltOn.Tests.Other;
using BoltOn.Tests.Requestor.Fakes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BoltOn.Tests.Requestor
{
    [Collection("IntegrationTests")]
	public class RequestorIntegrationTests : IDisposable
	{
		[Fact]
		public async Task Process_HandlerRegistrationsDisabled_ThrowsException()
		{
			// arrange
			IntegrationTestHelper.IsSeedData = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn(options =>
			{
				options.DisableRequestorHandlerRegistrations();
			});
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IRequestor>();
			var testCommand = new TestCommand();

			// act
			var result = await Record.ExceptionAsync(() =>
			{
				return sut.ProcessAsync(testCommand);
			});

			// assert 
			Assert.NotNull(result);
			Assert.Equal($"Handler not found for request: {testCommand.GetType()}", result.Message);
		}

		[Fact]
		public async Task Process_HandlerRegistrationsDisabledAndExplicitRegistration_ReturnsExpectedResult()
		{
			// arrange
			IntegrationTestHelper.IsSeedData = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn(options =>
			{
				options.DisableRequestorHandlerRegistrations();
			});
			serviceCollection.AddTransient<IHandler<TestCommand, bool>, TestHandler>();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IRequestor>();

			// act
			var result = await sut.ProcessAsync(new TestCommand());

			// assert 
			Assert.True(result);
		}

		public void Dispose()
		{
			RequestorTestHelper.IsCustomizeIsolationLevel = false;
			RequestorTestHelper.LoggerStatements.Clear();
			IntegrationTestHelper.IsSeedData = false;
		}
	}
}
