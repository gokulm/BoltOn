using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Bus;
using BoltOn.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BoltOn.Bus.RabbitMq;
using MassTransit.RabbitMqTransport;
using MassTransit;

namespace BoltOn.Tests.Bus
{
	[Collection("IntegrationTests")]
	public class MassTransitBoltOnBusIntegrationTests
	{
		public MassTransitBoltOnBusIntegrationTests()
		{
		}


		[Fact]
		public async void Publish_Message_GetsConsumed()
		{
			var services = new ServiceCollection();
			services.BoltOn(b =>
			{
				b.BoltOnAssemblies(GetType().Assembly);
			});


			services.BoltOnRabbitMqBus(o =>
			{
				o.HostAddress = "rabbitmq://localhost:5672";
				o.Username = "guest";
				o.Password = "guest";
			});

			var serviceProvider = services.BuildServiceProvider();

			//serviceProvider.UseRabbitMqBus(o =>
			//{
			//	o.HostAddress = "rabbitmq://localhost:5672";
			//	o.Username = "guest";
			//	o.Password = "guest";
			//}, typeof(CreateStudentHandler).Assembly);
			serviceProvider.TightenBolts();

			var bus = serviceProvider.GetService<BoltOn.Bus.IBus>();
			var cfg = serviceProvider.GetService<IRabbitMqBusFactoryConfigurator>();
			var host = cfg.Host(new Uri("rabbitmq://localhost:5672"), h =>
			{
				h.Username("guest");
				h.Password("password");
			});
			//cfg.ReceiveEndpoint(host, $"Test_queue", endpoint =>
			//{
			//	endpoint.Handler<CreateStudent>(async c =>
			//	{
			//		await Task.Delay(1000);
			//		Console.WriteLine("test jj");
			//	});
			//});


			await bus.PublishAsync(new CreateStudent { FirstName = "test" });

		}
	}

	public class CreateStudent : IMessage
	{
		public string FirstName { get; set; }
		public Guid CorrelationId { get; set; } = Guid.NewGuid();
	}


	public class CreateStudentHandler : IRequestAsyncHandler<CreateStudent>
	{
		public async Task HandleAsync(CreateStudent request, CancellationToken cancellationToken)
		{
			await Task.Delay(3000);
			await Task.FromResult("testing");
		}
	}
}
