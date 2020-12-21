using System;
using System.Linq;
using BoltOn.Bootstrapping;
using BoltOn.Cqrs;
using BoltOn.Requestor.Interceptors;
using BoltOn.Tests.Bootstrapping.Fakes;
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
			var boltOnOptions = new BoltOnOptions(serviceCollection.Object);

			// act
			boltOnOptions.AddInterceptor<StopwatchInterceptor>();
			boltOnOptions.AddInterceptor<UnitOfWorkInterceptor>();
			boltOnOptions.AddInterceptor<CqrsInterceptor>().After<StopwatchInterceptor>();

			// assert
			var interceptorTypes = boltOnOptions.InterceptorTypes.ToList();
			var stopWatchInterceptorIndex = interceptorTypes.IndexOf(typeof(StopwatchInterceptor));
			var uowInterceptorIndex = interceptorTypes.IndexOf(typeof(UnitOfWorkInterceptor));
			var cqrsInterceptorIndex = interceptorTypes.IndexOf(typeof(CqrsInterceptor));
			Assert.True(stopWatchInterceptorIndex != -1);
			Assert.True(uowInterceptorIndex != -1);
			Assert.True(cqrsInterceptorIndex != -1);
			Assert.True(cqrsInterceptorIndex < uowInterceptorIndex);
			Assert.True(cqrsInterceptorIndex > stopWatchInterceptorIndex);
		}

		[Fact]
		public void AddInterceptor_BeforeAnExistingInterceptor_ReturnsInterceptorsInExpectedOrder()
		{
			// arrange
			var serviceCollection = new Moq.Mock<IServiceCollection>();
			var boltOnOptions = new BoltOnOptions(serviceCollection.Object);

			// act
			boltOnOptions.AddInterceptor<StopwatchInterceptor>();
			boltOnOptions.AddInterceptor<UnitOfWorkInterceptor>();
			boltOnOptions.AddInterceptor<CqrsInterceptor>().Before<UnitOfWorkInterceptor>();

			// assert
			var interceptorTypes = boltOnOptions.InterceptorTypes.ToList();
			var stopWatchInterceptorIndex = interceptorTypes.IndexOf(typeof(StopwatchInterceptor));
			var uowInterceptorIndex = interceptorTypes.IndexOf(typeof(UnitOfWorkInterceptor));
			var cqrsInterceptorIndex = interceptorTypes.IndexOf(typeof(CqrsInterceptor));
			Assert.True(stopWatchInterceptorIndex != -1);
			Assert.True(uowInterceptorIndex != -1);
			Assert.True(cqrsInterceptorIndex != -1);
			Assert.True(cqrsInterceptorIndex < uowInterceptorIndex);
			Assert.True(cqrsInterceptorIndex > stopWatchInterceptorIndex);
		}

		[Fact]
		public void AddInterceptor_AfterTheLastInterceptor_ReturnsInterceptorsInExpectedOrder()
		{
			// arrange
			var serviceCollection = new Moq.Mock<IServiceCollection>();
			var boltOnOptions = new BoltOnOptions(serviceCollection.Object);

			// act
			boltOnOptions.AddInterceptor<TestBootstrappingInterceptor>().After<UnitOfWorkInterceptor>();

			// assert
			var interceptorTypes = boltOnOptions.InterceptorTypes.ToList();
			var stopWatchInterceptorIndex = interceptorTypes.IndexOf(typeof(StopwatchInterceptor));
			var testBootstrappingInterceptorIndex = interceptorTypes.IndexOf(typeof(TestBootstrappingInterceptor));
			var uowInterceptorIndex = interceptorTypes.IndexOf(typeof(UnitOfWorkInterceptor));
			Assert.True(stopWatchInterceptorIndex != -1);
			Assert.True(uowInterceptorIndex != -1);
			Assert.True(testBootstrappingInterceptorIndex != -1);
			Assert.True(testBootstrappingInterceptorIndex > uowInterceptorIndex);
		}

		[Fact]
		public void AddInterceptor_BeforeTheFirstInterceptor_ReturnsInterceptorsInExpectedOrder()
		{
			// arrange
			var serviceCollection = new Moq.Mock<IServiceCollection>();
			var boltOnOptions = new BoltOnOptions(serviceCollection.Object);

			// act
			boltOnOptions.AddInterceptor<TestBootstrappingInterceptor>().Before<StopwatchInterceptor>();

			// assert
			var interceptorTypes = boltOnOptions.InterceptorTypes.ToList();
			var stopWatchInterceptorIndex = interceptorTypes.IndexOf(typeof(StopwatchInterceptor));
			var testBootstrappingInterceptorIndex = interceptorTypes.IndexOf(typeof(TestBootstrappingInterceptor));
			var uowInterceptorIndex = interceptorTypes.IndexOf(typeof(UnitOfWorkInterceptor));
			Assert.True(stopWatchInterceptorIndex != -1);
			Assert.True(uowInterceptorIndex != -1);
			Assert.True(testBootstrappingInterceptorIndex != -1);
			Assert.True(testBootstrappingInterceptorIndex < stopWatchInterceptorIndex);
		}

		[Fact]
		public void AddInterceptor_BeforeAnAlreadyAddedInterceptor_ReturnsInterceptorsInExpectedOrder()
		{
			// arrange
			var serviceCollection = new Moq.Mock<IServiceCollection>();
			var boltOnOptions = new BoltOnOptions(serviceCollection.Object);

			// act
			boltOnOptions.AddInterceptor<UnitOfWorkInterceptor>().Before<StopwatchInterceptor>();

			// assert
			var interceptorTypes = boltOnOptions.InterceptorTypes.ToList();
			var stopWatchInterceptorIndex = interceptorTypes.IndexOf(typeof(StopwatchInterceptor));
			var uowInterceptorIndex = interceptorTypes.IndexOf(typeof(UnitOfWorkInterceptor));
			Assert.True(stopWatchInterceptorIndex != -1);
			Assert.True(uowInterceptorIndex != -1);
			Assert.True(uowInterceptorIndex < stopWatchInterceptorIndex);
		}

		[Fact]
		public void AddInterceptor_AfterAnAlreadyAddedInterceptor_ReturnsInterceptorsInExpectedOrder()
		{
			// arrange
			var serviceCollection = new Moq.Mock<IServiceCollection>();
			var boltOnOptions = new BoltOnOptions(serviceCollection.Object);

			// act
			boltOnOptions.AddInterceptor<StopwatchInterceptor>().After<UnitOfWorkInterceptor>();

			// assert
			var interceptorTypes = boltOnOptions.InterceptorTypes.ToList();
			var stopWatchInterceptorIndex = interceptorTypes.IndexOf(typeof(StopwatchInterceptor));
			var uowInterceptorIndex = interceptorTypes.IndexOf(typeof(UnitOfWorkInterceptor));
			Assert.True(stopWatchInterceptorIndex != -1);
			Assert.True(uowInterceptorIndex != -1);
			Assert.True(uowInterceptorIndex < stopWatchInterceptorIndex);
		}

		[Fact]
		public void AddInterceptor_BeforeAnInterceptorThatDoesntExist_ReturnsInterceptorsInExpectedOrder()
		{
			// arrange
			var serviceCollection = new Moq.Mock<IServiceCollection>();
			var boltOnOptions = new BoltOnOptions(serviceCollection.Object);

			// act
			boltOnOptions.AddInterceptor<StopwatchInterceptor>();
			boltOnOptions.AddInterceptor<CqrsInterceptor>().Before<TestBootstrappingInterceptor>();

			// assert
			var interceptorTypes = boltOnOptions.InterceptorTypes.ToList();
			var stopWatchInterceptorIndex = interceptorTypes.IndexOf(typeof(StopwatchInterceptor));
			var testBootstrappingInterceptorIndex = interceptorTypes.IndexOf(typeof(TestBootstrappingInterceptor));
			var cqrsInterceptorIndex = interceptorTypes.IndexOf(typeof(CqrsInterceptor));
			Assert.True(stopWatchInterceptorIndex != -1);
			Assert.True(testBootstrappingInterceptorIndex == -1);
			Assert.True(cqrsInterceptorIndex != -1);
			Assert.True(cqrsInterceptorIndex > stopWatchInterceptorIndex);
		}

		[Fact]
		public void AddInterceptor_AfterAnInterceptorThatDoesntExist_ReturnsInterceptorsInExpectedOrder()
		{
			// arrange
			var serviceCollection = new Moq.Mock<IServiceCollection>();
			var boltOnOptions = new BoltOnOptions(serviceCollection.Object);

			// act
			boltOnOptions.AddInterceptor<StopwatchInterceptor>();
			boltOnOptions.AddInterceptor<CqrsInterceptor>().After<TestBootstrappingInterceptor>();

			// assert
			var interceptorTypes = boltOnOptions.InterceptorTypes.ToList();
			var stopWatchInterceptorIndex = interceptorTypes.IndexOf(typeof(StopwatchInterceptor));
			var testBootstrappingInterceptorIndex = interceptorTypes.IndexOf(typeof(TestBootstrappingInterceptor));
			var cqrsInterceptorIndex = interceptorTypes.IndexOf(typeof(CqrsInterceptor));
			Assert.True(stopWatchInterceptorIndex != -1);
			Assert.True(testBootstrappingInterceptorIndex == -1);
			Assert.True(cqrsInterceptorIndex != -1);
			Assert.True(cqrsInterceptorIndex > stopWatchInterceptorIndex);
		}
	}
}
