using System;
using System.Collections.Generic;
using BoltOn.Bootstrapping;
using BoltOn.Logging;
using BoltOn.Mediator.Data.EF;
using BoltOn.Mediator.Middlewares;
using BoltOn.Mediator.Pipeline;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace BoltOn.Tests.Mediator.Data.EF
{
	public class MediatorDataEFTests : IDisposable
	{
		[Fact]
		public void Get_MediatorWithQueryRequest_ExecutesEFAutoDetectChangesDisablingMiddlewareAndDisablesAutoDetectChangesEnabled()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var serviceProvider = autoMocker.GetMock<IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			var middleware = new Mock<IMediatorMiddleware>();
			var logger = new Mock<IBoltOnLogger<EFAutoDetectChangesDisablingMiddleware>>();
			serviceProvider.Setup(s => s.GetService(typeof(IRequestHandler<TestQuery, bool>)))
				.Returns(testHandler.Object);
			var request = new TestQuery();
			var mediatorContext = new MediatorDataContext();
			autoMocker.Use<IEnumerable<IMediatorMiddleware>>(new List<IMediatorMiddleware>
			{
				new EFAutoDetectChangesDisablingMiddleware(logger.Object, mediatorContext)
			});
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Pipeline.Mediator>();
			testHandler.Setup(s => s.Handle(request)).Returns(true);

			// act
			var result = sut.Get(request);

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			Assert.False(mediatorContext.IsAutoDetectChangesEnabled);
			logger.Verify(l => l.Debug($"Entering {nameof(EFAutoDetectChangesDisablingMiddleware)}..."));
			logger.Verify(l => l.Debug($"IsAutoDetectChangesEnabled: {false}"));
		}

		[Fact]
		public void Get_MediatorWithCommandRequest_ExecutesEFAutoDetectChangesDisablingMiddlewareAndEnablesAutoDetectChangesEnabled()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var serviceProvider = autoMocker.GetMock<IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			var middleware = new Mock<IMediatorMiddleware>();
			var logger = new Mock<IBoltOnLogger<EFAutoDetectChangesDisablingMiddleware>>();
			serviceProvider.Setup(s => s.GetService(typeof(IRequestHandler<TestCommand, bool>)))
				.Returns(testHandler.Object);
			var request = new TestCommand();
			var mediatorContext = new MediatorDataContext();
			autoMocker.Use<IEnumerable<IMediatorMiddleware>>(new List<IMediatorMiddleware>
			{
				new EFAutoDetectChangesDisablingMiddleware(logger.Object, mediatorContext)
			});
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Pipeline.Mediator>();
			testHandler.Setup(s => s.Handle(request)).Returns(true);

			// act
			var result = sut.Get(request);

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			Assert.True(mediatorContext.IsAutoDetectChangesEnabled);
			logger.Verify(l => l.Debug($"Entering {nameof(EFAutoDetectChangesDisablingMiddleware)}..."));
			logger.Verify(l => l.Debug($"IsAutoDetectChangesEnabled: {true}"));
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
