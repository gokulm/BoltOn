using System.Threading.Tasks;
using Moq.AutoMock;
using Xunit;
using BoltOn.Bus.MassTransit;
using MassTransit;
using System.Threading;
using BoltOn.Logger;
using BoltOn.Tests.Bus.Fakes;

namespace BoltOn.Tests.Bus
{
	public class AppServiceBusTests 
	{
		[Fact]
		public async Task PublishAsync_Message_GetsPublished()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var busControl = autoMocker.GetMock<IBusControl>();
			var logger = autoMocker.GetMock<IAppLogger<AppServiceBus>>();
			var sut = autoMocker.CreateInstance<AppServiceBus>();
			var request = new CreateTestStudent();
			var cts = new CancellationTokenSource();

			// act
			await sut.PublishAsync(request, cts.Token);

			// assert 
			logger.Verify(l => l.Debug($"Publishing message of type - {request.GetType().Name} ..."));
			logger.Verify(l => l.Debug("Message published"));
			// todo: fix this verification
			//busControl.Verify(m => m.Publish(request, cts.Token));
		}
	}
}
