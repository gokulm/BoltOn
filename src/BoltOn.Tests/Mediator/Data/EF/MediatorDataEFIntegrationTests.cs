using System;
using System.Linq;
using BoltOn.Bootstrapping;
using BoltOn.Mediator.Data.EF;
using BoltOn.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltOn.Tests.Mediator.Data.EF
{
	[Collection("IntegrationTests")]
	public class MediatorDataEFIntegrationTests : IDisposable
	{
		[Fact]
		public void Get_MediatorWithQueryRequest_ExecutesEFAutoDetectChangesDisablingMiddleware()
		{
			// arrange
			MediatorTestHelper.IsCustomizeIsolationLevel = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn()
				.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseBoltOn();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = sut.Get(new TestQuery());

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Entering {nameof(EFAutoDetectChangesDisablingMiddleware)}..."));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"IsAutoDetectChangesEnabled: {false}"));
		}

		public void Dispose()
		{
			MediatorTestHelper.LoggerStatements.Clear();
			Bootstrapper
				.Instance
				.Dispose();
		}
	}
}
