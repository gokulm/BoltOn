using System.Threading.Tasks;
using BoltOn.Tests.Other;
using Moq.AutoMock;
using Xunit;
using BoltOn.Bus.MassTransit;
using MassTransit;
using System.Threading;
using BoltOn.Logging;

namespace BoltOn.Tests.Bus
{
	public class BoltOnMassTransitBusTests 
	{
		[Fact]
		public async Task PublishAsync_Message_GetsPublished()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var busControl = autoMocker.GetMock<IBusControl>();
			var logger = autoMocker.GetMock<IBoltOnLogger<BoltOnMassTransitBus>>();
			var sut = autoMocker.CreateInstance<BoltOnMassTransitBus>();
			var request = new CreateTestStudent();
			var cts = new CancellationTokenSource();

			// act
			await sut.PublishAsync(request, cts.Token);

			// assert 
			logger.Verify(l => l.Debug($"Publishing message of type - {request.GetType().Name} ..."));
			logger.Verify(l => l.Debug("Message published"));
			//busControl.Verify(m => m.Publish(request, cts.Token));
		}
	}
}
