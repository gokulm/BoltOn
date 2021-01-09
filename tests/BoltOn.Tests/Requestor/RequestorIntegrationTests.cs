using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using BoltOn.Requestor.Pipeline;
using BoltOn.Tests.Other;
using BoltOn.Tests.Requestor.Fakes;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltOn.Tests.Requestor
{
	[Collection("IntegrationTests")]
	public class RequestorIntegrationTests : IDisposable
	{
		[Fact]
		public async Task Process_BootstrapWithDefaults_InvokesAllTheInterceptorsAndReturnsSuccessfulResult()
		{
			// arrange
			RequestorTestHelper.IsRemoveStopwatchInterceptor = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn(b => b.RegisterRequestorFakes());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var appClock = serviceProvider.GetService<IAppClock>();
			var requestor = serviceProvider.GetService<IRequestor>();

			// act
			var result = await requestor.ProcessAsync(new TestRequest());

			// assert 
			Assert.True(result);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {appClock.Now}"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {appClock.Now}. Time elapsed: 0"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == "Request didn't enable transaction"));
		}

		[Fact]
		public async Task Process_BootstrapWithDefaults_InvokesAllTheInterceptorsAndReturnsSuccessfulResultForOneWayRequest()
		{
			// arrange
			RequestorTestHelper.IsRemoveStopwatchInterceptor = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn(b => b.RegisterRequestorFakes());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var appClock = serviceProvider.GetService<IAppClock>();
			var requestor = serviceProvider.GetService<IRequestor>();
			var request = new TestOneWayRequest();

			// act
			await requestor.ProcessAsync(request);

			// assert 
			Assert.Equal(1, request.Value);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {appClock.Now}"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {appClock.Now}. Time elapsed: 0"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == "Request didn't enable transaction"));
		}

		[Fact]
		public async Task Process_BootstrapWithDefaults_InvokesAllTheInterceptorsAndReturnsSuccessfulResultForOneWayCommand()
		{
			// arrange
			RequestorTestHelper.IsRemoveStopwatchInterceptor = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
            serviceCollection.BoltOn(b => b.RegisterRequestorFakes());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var appClock = serviceProvider.GetService<IAppClock>();
			var requestor = serviceProvider.GetService<IRequestor>();
			var request = new TestOneWayCommand();

			// act
			await requestor.ProcessAsync(request);

			// assert 
			Assert.Equal(1, request.Value);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {appClock.Now}"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {appClock.Now}. Time elapsed: 0"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"About to start transaction. TransactionScopeOption: {TransactionScopeOption.RequiresNew} " +
					$"IsolationLevel: {IsolationLevel.ReadCommitted} Timeout: { TransactionManager.DefaultTimeout}"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == "Transaction completed"));
		}

		[Fact]
		public async Task Process_BootstrapWithDefaults_InvokesAllTheInterceptorsAndReturnsSuccessfulResultForOneWayAsyncRequest()
		{
			// arrange
			RequestorTestHelper.IsRemoveStopwatchInterceptor = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
            serviceCollection.BoltOn(b => b.RegisterRequestorFakes());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var appClock = serviceProvider.GetService<IAppClock>();
			var requestor = serviceProvider.GetService<IRequestor>();
			var request = new TestOneWayRequest();

			// act
			await requestor.ProcessAsync(request);

			// assert 
			Assert.Equal(1, request.Value);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {appClock.Now}"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {appClock.Now}. Time elapsed: 0"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == "Request didn't enable transaction"));
		}

		[Fact]
		public async Task Process_BootstrapWithDefaultsAndAsyncHandler_InvokesAllTheInterceptorsAndReturnsSuccessfulResult()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
            serviceCollection.BoltOn(b => b.RegisterRequestorFakes());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var appClock = serviceProvider.GetService<IAppClock>();
			var requestor = serviceProvider.GetService<IRequestor>();

			// act
			CancellationTokenSource cts = new CancellationTokenSource();
			cts.Cancel();
			CancellationToken token = cts.Token;
			var result = await requestor.ProcessAsync(new TestRequest(), token);

			// assert 
			Assert.True(result);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {appClock.Now}"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {appClock.Now}. Time elapsed: 0"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == "Request didn't enable transaction"));
		}

		[Fact]
		public async Task Process_BootstrapWithTestInterceptorsAndAsyncHandler_InvokesDefaultAndTestInterceptorInOrderAndReturnsSuccessfulResult()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
            serviceCollection.BoltOn(b => b.RegisterRequestorFakes());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var appClock = serviceProvider.GetService<IAppClock>();
			var requestor = serviceProvider.GetService<IRequestor>();

			// act
			var result = await requestor.ProcessAsync(new TestRequest());

			// assert 
			Assert.True(result);
			Assert.True(RequestorTestHelper.LoggerStatements.IndexOf("TestInterceptor Started") > 0);
			Assert.True(RequestorTestHelper.LoggerStatements.IndexOf("TestInterceptor Ended") > 0);
			Assert.True(RequestorTestHelper.LoggerStatements.IndexOf("TestRequestSpecificInterceptor Started") == -1);
			Assert.True(RequestorTestHelper.LoggerStatements.IndexOf($"StopwatchInterceptor started at {appClock.Now}") <
						RequestorTestHelper.LoggerStatements.IndexOf("TestInterceptor Started"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor started at {appClock.Now}"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor ended at {appClock.Now}. " +
																				   "Time elapsed: 0"));
			Assert.True(RequestorTestHelper.LoggerStatements.IndexOf($"StopwatchInterceptor ended at {appClock.Now}. Time elapsed: 0") >
						RequestorTestHelper.LoggerStatements.IndexOf("TestInterceptor Ended"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == "Request didn't enable transaction"));
		}

		[Fact]
		public async Task Process_BootstrapWithTestInterceptorsAndRemoveAll_InvokesOnlyTestInterceptorAndReturnsSuccessfulResult()
		{
			// arrange
			RequestorTestHelper.IsClearInterceptors = true;
			var serviceCollection = new ServiceCollection();
            serviceCollection.BoltOn(b => b.RegisterRequestorFakes());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var appClock = serviceProvider.GetService<IAppClock>();
			var sut = serviceProvider.GetService<IRequestor>();

			// act
			var result = await sut.ProcessAsync(new TestRequest());

			// assert 
			Assert.True(result);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestInterceptor Started"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestInterceptor Ended"));
			Assert.Null(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor started at {appClock.Now}"));
			Assert.Null(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor ended at {appClock.Now}. " +
																				"Time elapsed: 0"));
		}

		[Fact]
		public async Task Process_BootstrapWithRemoveInterceptor_DoesNotInvokeRemovedInterceptorAndReturnsSuccessfulResult()
		{
			// arrange
			RequestorTestHelper.IsRemoveStopwatchInterceptor = true;
			var serviceCollection = new ServiceCollection();
            serviceCollection.BoltOn(b => b.RegisterRequestorFakes());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var appClock = serviceProvider.GetService<IAppClock>();
			var sut = serviceProvider.GetService<IRequestor>();

			// act
			var result = await sut.ProcessAsync(new TestRequest());

			// assert 
			Assert.True(result);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestInterceptor Started"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestInterceptor Ended"));
			Assert.Null(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor started at {appClock.Now}"));
			Assert.Null(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor ended at {appClock.Now}. " +
																				"Time elapsed: 0"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == "Request didn't enable transaction"));
		}

		[Fact]
		public async Task Process_HandlerRegistrationsDisabled_ThrowsException()
		{
			// arrange
			IntegrationTestHelper.IsSeedData = false;
			var serviceCollection = new ServiceCollection();
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
			RequestorTestHelper.IsRemoveStopwatchInterceptor = false;
			RequestorTestHelper.IsClearInterceptors = false;
			RequestorTestHelper.IsCustomizeIsolationLevel = false;
			RequestorTestHelper.LoggerStatements.Clear();
			IntegrationTestHelper.IsSeedData = false;
		}
	}
}
