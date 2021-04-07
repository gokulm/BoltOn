using System;
using System.IO;
using BoltOn.Bus.MassTransit;
using BoltOn.Data.EF;
using BoltOn.Hangfire;
using BoltOn.Samples.Application.Entities;
using BoltOn.Samples.Application.Handlers;
using Hangfire;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

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
				o.BoltOnEFModule();
				o.BoltOnHangfireModule();
				o.BoltOnMassTransitBusModule();
			});

			Log.Logger = new LoggerConfiguration()
							.Enrich.WithMachineName()
							.ReadFrom.Configuration(configuration)
							.CreateLogger();

			serviceCollection.AddLogging(builder => builder.AddSerilog());

			var boltOnSamplesDbConnectionString = configuration.GetValue<string>("BoltOnSamplesDbConnectionString");

			GlobalConfiguration.Configuration
			 .UseSqlServerStorage(boltOnSamplesDbConnectionString);


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
						ep.Consumer(() => provider.GetService<AppMessageConsumer<StudentCreatedEvent>>());
					});
				}));
			});

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();

			RecurringJob.AddOrUpdate<AppHangfireJobProcessor>("StudentsNotifier",
				p => p.ProcessAsync(new NotifyStudentsRequest { JobType = "Recurring" }, default), Cron.Minutely());

			BackgroundJob.Schedule<AppHangfireJobProcessor>(p => p.ProcessAsync(new NotifyStudentsRequest { JobType = "OneTime" }, default),
				TimeSpan.FromSeconds(30));

			using var hangfireServer = new BackgroundJobServer();
			System.Console.ReadLine();
		}
	}
}
