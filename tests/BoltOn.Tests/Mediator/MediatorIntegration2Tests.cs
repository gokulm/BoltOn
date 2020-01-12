using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Bootstrapping;
using BoltOn.Mediator.Pipeline;
using BoltOn.Tests.Common;
using BoltOn.Tests.Mediator.Fakes;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltOn.Tests.Mediator
{
	[Collection("IntegrationTests")]
	[TestCaseOrderer("BoltOn.Tests.Common.PriorityOrderer", "BoltOn.Tests")]
	public class MediatorIntegration2Tests : IClassFixture<MediatorIntegration2TestFixture>
	{
		private static IServiceCollection _serviceCollection;
		private static IServiceProvider _serviceProvider;
		private static IBoltOnClock _boltOnClock;
		private static IMediator _sut;

		static MediatorIntegration2Tests()
		{
			_serviceCollection = new ServiceCollection();
			_serviceCollection.AddLogging();
			_serviceCollection.BoltOn();
			_serviceProvider = _serviceCollection.BuildServiceProvider();
			_serviceProvider.TightenBolts();
			_boltOnClock = _serviceProvider.GetService<IBoltOnClock>();
			_sut = _serviceProvider.GetService<IMediator>();
		}

		[Fact]
		[TestPriority(3)]
		public async Task Process_BootstrapWithDefaults_InvokesAllTheInterceptorsAndReturnsSuccessfulResult()
		{
			// arrange

			// act
			var result = await _sut.ProcessAsync(new TestRequest());

			// assert 
			Assert.True(result);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {_boltOnClock.Now}"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {_boltOnClock.Now}. Time elapsed: 0"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
		}


		[Fact]
		[TestPriority(6)]
		public async Task Process_BootstrapWithDefaults_InvokesAllTheInterceptorsAndReturnsSuccessfulResultForOneWayRequest()
		{
			// arrange
			var request = new TestOneWayRequest();

			// act
			await _sut.ProcessAsync(request);

			// assert 
			Assert.Equal(1, request.Value);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {_boltOnClock.Now}"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {_boltOnClock.Now}. Time elapsed: 0"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
		}


		[Fact]
		[TestPriority(9)]
		public async Task Process_BootstrapWithDefaultsAndCancellationToken_InvokesAllTheInterceptorsAndReturnsSuccessfulResult()
		{
			// arrange

			// act
			CancellationTokenSource cts = new CancellationTokenSource();
			cts.Cancel();
			CancellationToken token = cts.Token;
			var result = await _sut.ProcessAsync(new TestRequest(), token);

			// assert 
			Assert.True(result);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {_boltOnClock.Now}"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {_boltOnClock.Now}. Time elapsed: 0"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
		}

		[Fact]
		[TestPriority(12)]
		public async Task Process_BootstrapWithTestInterceptors_InvokesDefaultAndTestInterceptorInOrderAndReturnsSuccessfulResult()
		{
			// arrange

			// act
			var result = await _sut.ProcessAsync(new TestRequest());

			// assert 
			Assert.True(result);
			Assert.True(MediatorTestHelper.LoggerStatements.IndexOf("TestInterceptor Started") > 0);
			Assert.True(MediatorTestHelper.LoggerStatements.IndexOf("TestInterceptor Ended") > 0);
			Assert.True(MediatorTestHelper.LoggerStatements.IndexOf("TestRequestSpecificInterceptor Started") == -1);
			Assert.True(MediatorTestHelper.LoggerStatements.IndexOf($"StopwatchInterceptor started at {_boltOnClock.Now}") <
						MediatorTestHelper.LoggerStatements.IndexOf("TestInterceptor Started"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor started at {_boltOnClock.Now}"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor ended at {_boltOnClock.Now}. " +
																				   "Time elapsed: 0"));
			Assert.True(MediatorTestHelper.LoggerStatements.IndexOf($"StopwatchInterceptor ended at {_boltOnClock.Now}. Time elapsed: 0") >
						MediatorTestHelper.LoggerStatements.IndexOf("TestInterceptor Ended"));
		}
	}

	public class MediatorIntegration2TestFixture : IDisposable
	{
		public MediatorIntegration2TestFixture()
		{
			MediatorTestHelper.IsRemoveStopwatchInterceptor = false;
			MediatorTestHelper.IsClearInterceptors = false;
			MediatorTestHelper.IsCustomizeIsolationLevel = false;
			MediatorTestHelper.LoggerStatements.Clear();
		}

		public void Dispose()
		{
			Bootstrapper
				.Instance
				.Dispose();
		}
	}
}
