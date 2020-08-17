using System;
using System.IO;
using BoltOn.Samples.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Bus.MassTransit;
using Microsoft.Extensions.Logging;
using BoltOn.Data.EF;
using BoltOn.Samples.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BoltOn.Data;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Console
{
	class Program
	{
		static void Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("CONSOLE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json");

            System.Console.WriteLine($"Environment: {environment}");

            if (!string.IsNullOrEmpty(environment))
                builder.AddJsonFile($"appsettings.{environment}.json");

            var configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging(configure => configure.AddConsole());
			serviceCollection.BoltOn(o =>
			{
				o.BoltOnAssemblies(typeof(GetAllStudentsRequest).Assembly);
				o.BoltOnMassTransitBusModule();
				o.BoltOnCqrsModule(b => b.PurgeEventsProcessedBefore = TimeSpan.FromHours(12));
				o.BoltOnEFModule();
			});

            var readDbConnectionString = configuration.GetValue<string>("SqlReadDbConnectionString");
            var rabbitmqUri = configuration.GetValue<string>("RabbitMqUri");
            var rabbitmqUsername = configuration.GetValue<string>("RabbitMqUsername");
            var rabbitmqPassword = configuration.GetValue<string>("RabbitMqPassword");

            serviceCollection.AddMassTransit(x =>
            {
                x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(new Uri(rabbitmqUri), hostConfigurator =>
                    {
                        hostConfigurator.Username(rabbitmqUsername);
                        hostConfigurator.Password(rabbitmqPassword);
                    });

                    cfg.ReceiveEndpoint("StudentCreatedEvent_queue", ep =>
                    {
                        ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<StudentCreatedEvent>>());
                    });

                    cfg.ReceiveEndpoint("StudentUpdatedEvent_queue", ep =>
                    {
                        ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<StudentUpdatedEvent>>());
                    });
                }));
            });

            serviceCollection.AddDbContext<SchoolReadDbContext>(options =>
            {
                options.UseSqlServer(readDbConnectionString);
            });

			serviceCollection.AddTransient<IRepository<StudentFlattened>, CqrsRepository<StudentFlattened, SchoolReadDbContext>>();

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			System.Console.ReadLine();
		}
	}
}
