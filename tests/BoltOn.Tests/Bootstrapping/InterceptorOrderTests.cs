using System;
using System.Linq;
using BoltOn.Bootstrapping;
using BoltOn.Cqrs;
using BoltOn.Requestor.Interceptors;
using BoltOn.Tests.Bootstrapping.Fakes;
using BoltOn.Transaction;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltOn.Tests.Bootstrapping
{
	public class InterceptorOrderTests
	{
		[Fact]
		public void AddInterceptor_AfterAnExistingInterceptor_ReturnsInterceptorsInExpectedOrder()
		{
			// arrange
			var serviceCollection = new Moq.Mock<IServiceCollection>();
			var bootstrapperOptions = new BootstrapperOptions(serviceCollection.Object);

			// act
			bootstrapperOptions.AddInterceptor<StopwatchInterceptor>();
			bootstrapperOptions.AddInterceptor<TransactionInterceptor>();

			// assert
			var interceptorTypes = bootstrapperOptions.InterceptorTypes.ToList();
			var stopWatchInterceptorIndex = interceptorTypes.IndexOf(typeof(StopwatchInterceptor));
			var transactionInterceptorIndex = interceptorTypes.IndexOf(typeof(TransactionInterceptor));
			Assert.True(stopWatchInterceptorIndex != -1);
			Assert.True(transactionInterceptorIndex != -1);
		}

		[Fact]
		public void AddInterceptor_BeforeAnExistingInterceptor_ReturnsInterceptorsInExpectedOrder()
		{
			// arrange
			var serviceCollection = new Moq.Mock<IServiceCollection>();
			var bootstrapperOptions = new BootstrapperOptions(serviceCollection.Object);

			// act
			bootstrapperOptions.AddInterceptor<StopwatchInterceptor>();
			bootstrapperOptions.AddInterceptor<TransactionInterceptor>();

			// assert
			var interceptorTypes = bootstrapperOptions.InterceptorTypes.ToList();
			var stopWatchInterceptorIndex = interceptorTypes.IndexOf(typeof(StopwatchInterceptor));
			var transactionInterceptorIndex = interceptorTypes.IndexOf(typeof(TransactionInterceptor));
			Assert.True(stopWatchInterceptorIndex != -1);
			Assert.True(transactionInterceptorIndex != -1);
		}

		[Fact]
		public void AddInterceptor_AfterTheLastInterceptor_ReturnsInterceptorsInExpectedOrder()
		{
			// arrange
			var serviceCollection = new Moq.Mock<IServiceCollection>();
			var bootstrapperOptions = new BootstrapperOptions(serviceCollection.Object);

			// act
			bootstrapperOptions.AddInterceptor<TestBootstrappingInterceptor>().After<TransactionInterceptor>();

			// assert
			var interceptorTypes = bootstrapperOptions.InterceptorTypes.ToList();
			var stopWatchInterceptorIndex = interceptorTypes.IndexOf(typeof(StopwatchInterceptor));
			var testBootstrappingInterceptorIndex = interceptorTypes.IndexOf(typeof(TestBootstrappingInterceptor));
			var transactionInterceptorIndex = interceptorTypes.IndexOf(typeof(TransactionInterceptor));
			Assert.True(stopWatchInterceptorIndex != -1);
			Assert.True(transactionInterceptorIndex != -1);
			Assert.True(testBootstrappingInterceptorIndex != -1);
			Assert.True(testBootstrappingInterceptorIndex > transactionInterceptorIndex);
		}

		[Fact]
		public void AddInterceptor_BeforeTheFirstInterceptor_ReturnsInterceptorsInExpectedOrder()
		{
			// arrange
			var serviceCollection = new Moq.Mock<IServiceCollection>();
			var bootstrapperOptions = new BootstrapperOptions(serviceCollection.Object);

			// act
			bootstrapperOptions.AddInterceptor<TestBootstrappingInterceptor>().Before<StopwatchInterceptor>();

			// assert
			var interceptorTypes = bootstrapperOptions.InterceptorTypes.ToList();
			var stopWatchInterceptorIndex = interceptorTypes.IndexOf(typeof(StopwatchInterceptor));
			var testBootstrappingInterceptorIndex = interceptorTypes.IndexOf(typeof(TestBootstrappingInterceptor));
			var transactionInterceptorIndex = interceptorTypes.IndexOf(typeof(TransactionInterceptor));
			Assert.True(stopWatchInterceptorIndex != -1);
			Assert.True(transactionInterceptorIndex != -1);
			Assert.True(testBootstrappingInterceptorIndex != -1);
			Assert.True(testBootstrappingInterceptorIndex < stopWatchInterceptorIndex);
		}

		[Fact]
		public void AddInterceptor_BeforeAnAlreadyAddedInterceptor_ReturnsInterceptorsInExpectedOrder()
		{
			// arrange
			var serviceCollection = new Moq.Mock<IServiceCollection>();
			var bootstrapperOptions = new BootstrapperOptions(serviceCollection.Object);

			// act
			bootstrapperOptions.AddInterceptor<TransactionInterceptor>().Before<StopwatchInterceptor>();

			// assert
			var interceptorTypes = bootstrapperOptions.InterceptorTypes.ToList();
			var stopWatchInterceptorIndex = interceptorTypes.IndexOf(typeof(StopwatchInterceptor));
			var transactionInterceptorIndex = interceptorTypes.IndexOf(typeof(TransactionInterceptor));
			Assert.True(stopWatchInterceptorIndex != -1);
			Assert.True(transactionInterceptorIndex != -1);
			Assert.True(transactionInterceptorIndex < stopWatchInterceptorIndex);
		}

		[Fact]
		public void AddInterceptor_AfterAnAlreadyAddedInterceptor_ReturnsInterceptorsInExpectedOrder()
		{
			// arrange
			var serviceCollection = new Moq.Mock<IServiceCollection>();
			var bootstrapperOptions = new BootstrapperOptions(serviceCollection.Object);

			// act
			bootstrapperOptions.AddInterceptor<StopwatchInterceptor>().After<TransactionInterceptor>();

			// assert
			var interceptorTypes = bootstrapperOptions.InterceptorTypes.ToList();
			var stopWatchInterceptorIndex = interceptorTypes.IndexOf(typeof(StopwatchInterceptor));
			var transactionInterceptorIndex = interceptorTypes.IndexOf(typeof(TransactionInterceptor));
			Assert.True(stopWatchInterceptorIndex != -1);
			Assert.True(transactionInterceptorIndex != -1);
			Assert.True(transactionInterceptorIndex < stopWatchInterceptorIndex);
		}

		[Fact]
		public void AddInterceptor_BeforeAnInterceptorThatDoesntExist_ReturnsInterceptorsInExpectedOrder()
		{
			// arrange
			var serviceCollection = new Moq.Mock<IServiceCollection>();
			var bootstrapperOptions = new BootstrapperOptions(serviceCollection.Object);

			// act
			bootstrapperOptions.AddInterceptor<StopwatchInterceptor>();

			// assert
			var interceptorTypes = bootstrapperOptions.InterceptorTypes.ToList();
			var stopWatchInterceptorIndex = interceptorTypes.IndexOf(typeof(StopwatchInterceptor));
			var testBootstrappingInterceptorIndex = interceptorTypes.IndexOf(typeof(TestBootstrappingInterceptor));
			Assert.True(stopWatchInterceptorIndex != -1);
			Assert.True(testBootstrappingInterceptorIndex == -1);
		}

		[Fact]
		public void AddInterceptor_AfterAnInterceptorThatDoesntExist_ReturnsInterceptorsInExpectedOrder()
		{
			// arrange
			var serviceCollection = new Moq.Mock<IServiceCollection>();
			var bootstrapperOptions = new BootstrapperOptions(serviceCollection.Object);

			// act
			bootstrapperOptions.AddInterceptor<StopwatchInterceptor>();

			// assert
			var interceptorTypes = bootstrapperOptions.InterceptorTypes.ToList();
			var stopWatchInterceptorIndex = interceptorTypes.IndexOf(typeof(StopwatchInterceptor));
			var testBootstrappingInterceptorIndex = interceptorTypes.IndexOf(typeof(TestBootstrappingInterceptor));
			Assert.True(stopWatchInterceptorIndex != -1);
			Assert.True(testBootstrappingInterceptorIndex == -1);
		}
	}
}
