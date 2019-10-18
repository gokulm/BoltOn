using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Bus.MassTransit;
using MassTransit;
using System;
using Microsoft.Extensions.Logging;
using BoltOn.Data;
using BoltOn.Samples.Application.Entities;
using BoltOn.Data.EF;
using BoltOn.Samples.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
				o.BoltOnCqrsModule();
				o.BoltOnEFModule();
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

					cfg.ReceiveEndpoint("StudentCreatedEvent_queue", ep =>
					{
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<StudentCreatedEvent>>());
					});
				}));
			});

			serviceCollection.AddDbContext<SchoolDbContext>(options =>
			{
				options.UseSqlServer("Data Source=127.0.0.1;initial catalog=Testing;persist security info=True;User ID=sa;Password=Password1;");
			});

			serviceCollection.AddTransient<IRepository<StudentFlattened>, CqrsRepository<StudentFlattened, SchoolDbContext>>();

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();

			var mediator = serviceProvider.GetRequiredService<IMediator>();
			var response = mediator.Process(new PingRequest());
			System.Console.WriteLine(response.Data);
		}
	}
}
