using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Bus.MassTransit;
using MassTransit;
using System;
using Microsoft.Extensions.Logging;

namespace BoltOn.Samples.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging(configure => configure.AddConsole());
			serviceCollection.BoltOn(o =>
			{
				o.BoltOnAssemblies(typeof(GetAllStudentsRequest).Assembly);
				o.BoltOnMassTransitBusModule();
			});

			serviceCollection.AddMassTransit(x =>
			{
				x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
				{
					var host = cfg.Host(new Uri("rabbitmq://localhost:5672"), hostConfigurator =>
					{
						hostConfigurator.Username("guest");
						hostConfigurator.Password("guest");
					});

					cfg.ReceiveEndpoint(host, "CreateStudent_Queue", endpoint =>
					{
						endpoint.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<CreateStudentRequest>>());
					});
				}));
			});

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();

			var mediator = serviceProvider.GetRequiredService<IMediator>();
			var response = mediator.Process(new PingRequest());
			System.Console.WriteLine(response.Data);
		}
	}
}
