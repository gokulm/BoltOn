using System.Threading.Tasks;
using BoltOn.Tests.Other;
using Moq.AutoMock;
using Xunit;
using BoltOn.Bus.RabbitMq;
using MassTransit;
using Moq;
using System.Threading;

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
			var sut = autoMocker.CreateInstance<BoltOnMassTransitBus>();
			var request = new CreateTestStudent();
			var cts = new CancellationTokenSource();
			//busControl.Setup(s => s.Publish(request, cts.Token));//.Verifiable();

			// act
			await sut.PublishAsync(request, cts.Token);

			// assert 
			busControl.Verify(m => m.Publish(request, cts.Token));
		}
	}
}
