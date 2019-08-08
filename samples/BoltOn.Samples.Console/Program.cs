using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Bus.RabbitMq;
using MassTransit.RabbitMqTransport;
using System;
using MassTransit;
using BoltOn.Samples.Application.Messages;
using System.Threading.Tasks;

namespace BoltOn.Samples.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(o =>
			{
				o.BoltOnAssemblies(typeof(GetAllStudentsRequest).Assembly);
			});

			serviceCollection.BoltOnRabbitMqBus(o =>
			{
				o.HostAddress = "rabbitmq://localhost:5672";
				o.Username = "guest";
				o.Password = "guest";
			});

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();

			//serviceProvider.UseRabbitMqBus(o =>
			//{
			//	o.HostAddress = "rabbitmq://localhost:5672";
			//	o.Username = "guest";
			//	o.Password = "guest";
			//}, typeof(CreateStudentHandler).Assembly);

			var busFactoryConfigurator = serviceProvider.GetService<IRabbitMqBusFactoryConfigurator>();


			var host = busFactoryConfigurator.Host(new Uri("rabbitmq://localhost:5672"), h =>
			{
				h.Username("guest");
				h.Password("guest");
			});

			busFactoryConfigurator.ReceiveEndpoint(host, $"Test_queue", endpoint =>
			{
				endpoint.Handler<CreateStudent>(async context =>
				{
					await Task.Delay(0);
					System.Console.WriteLine($"Received: {context.Message}");
				});
			});
			var bus = serviceProvider.GetService<IBusControl>();
			bus.Start();

			var mediator = serviceProvider.GetRequiredService<IMediator>();
			var response = mediator.Process(new PingRequest());
			System.Console.WriteLine(response.Data);
		}
	}
}
