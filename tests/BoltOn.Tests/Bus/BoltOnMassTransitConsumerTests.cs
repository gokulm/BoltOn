using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Bus.RabbitMq;
using BoltOn.Mediator.Pipeline;
using BoltOn.Tests.Other;
using MassTransit;
using Moq;
using Moq.AutoMock;
using Xunit;
using BoltOn.Logging;

namespace BoltOn.Tests.Bus
{
	public class BoltOnMassTransitConsumerTests
	{
		[Fact]
		public async Task Consume_ValidRequestObject_GetsConsumed()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<BoltOnMassTransitConsumer<CreateTestStudent>>();
			var mediator = autoMocker.GetMock<IMediator>();
			var logger = autoMocker.GetMock<IBoltOnLogger<BoltOnMassTransitConsumer<CreateTestStudent>>>();
			var consumerContext = new Mock<ConsumeContext<CreateTestStudent>>();
			var request = new CreateTestStudent();
			consumerContext.Setup(c => c.Message).Returns(request);

			// act
			await sut.Consume(consumerContext.Object);

			// assert 
			logger.Verify(l => l.Debug($"Message of type {request.GetType().Name} consumer. " +
				"Sending to mediatiator..."));
			logger.Verify(l => l.Debug("Message sent to Mediator"));
			mediator.Verify(m => m.ProcessAsync(request, It.IsAny<CancellationToken>()));
		}
	}
}
