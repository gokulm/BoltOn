using System;
using System.Collections.Generic;
using BoltOn.Bootstrapping;
using BoltOn.Logging;
using BoltOn.Mediator.Data.EF;
using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace BoltOn.Tests.Mediator.Data.EF
{
	public class MediatorDataEFTests : IDisposable
	{
		[Fact]
		public void Get_MediatorWithQueryRequest_ExecutesEFQueryTrackingBehaviorInterceptorAndDisablesTracking()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var serviceProvider = autoMocker.GetMock<IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			var interceptor = new Mock<IInterceptor>();
			var logger = new Mock<IBoltOnLogger<EFQueryTrackingBehaviorInterceptor>>();
			serviceProvider.Setup(s => s.GetService(typeof(IRequestHandler<TestQuery, bool>)))
				.Returns(testHandler.Object);
			var request = new TestQuery();
			var mediatorContext = new MediatorDataContext();
			autoMocker.Use<IEnumerable<IInterceptor>>(new List<IInterceptor>
			{
				new EFQueryTrackingBehaviorInterceptor(logger.Object, mediatorContext)
			});
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Pipeline.Mediator>();
			testHandler.Setup(s => s.Handle(request)).Returns(true);

			// act
			var result = sut.Get(request);

			// assert 
			Assert.True(result);
			Assert.True(mediatorContext.IsQueryRequest);
			logger.Verify(l => l.Debug($"Entering {nameof(EFQueryTrackingBehaviorInterceptor)}..."));
			logger.Verify(l => l.Debug($"IsQueryRequest: {true}"));
		}

		[Fact]
		public void Get_MediatorWithCommandRequest_ExecutesEFQueryTrackingBehaviorInterceptorAndEnablesTracking()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var serviceProvider = autoMocker.GetMock<IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			var interceptor = new Mock<IInterceptor>();
			var logger = new Mock<IBoltOnLogger<EFQueryTrackingBehaviorInterceptor>>();
			serviceProvider.Setup(s => s.GetService(typeof(IRequestHandler<TestCommand, bool>)))
				.Returns(testHandler.Object);
			var request = new TestCommand();
			var mediatorContext = new MediatorDataContext();
			autoMocker.Use<IEnumerable<IInterceptor>>(new List<IInterceptor>
			{
				new EFQueryTrackingBehaviorInterceptor(logger.Object, mediatorContext)
			});
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Pipeline.Mediator>();
			testHandler.Setup(s => s.Handle(request)).Returns(true);

			// act
			var result = sut.Get(request);

			// assert 
			Assert.True(result);
			Assert.False(mediatorContext.IsQueryRequest);
			logger.Verify(l => l.Debug($"Entering {nameof(EFQueryTrackingBehaviorInterceptor)}..."));
			logger.Verify(l => l.Debug($"IsQueryRequest: {false}"));
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
