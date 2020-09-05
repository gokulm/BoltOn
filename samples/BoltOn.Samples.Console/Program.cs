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
using Hangfire;
using Hangfire.SqlServer;
using BoltOn.Hangfire;

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
				o.BoltOnHangfireModule(configuration =>
					configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
					.UseColouredConsoleLogProvider()
					.UseSimpleAssemblyNameTypeSerializer()
					.UseRecommendedSerializerSettings()
					.UseSqlServerStorage("Data Source=127.0.0.1,5005;initial catalog=HangfireTest;persist security info=True;User ID=sa;Password=Password1;", new SqlServerStorageOptions
					{
						CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
						SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
						QueuePollInterval = TimeSpan.Zero,
						UseRecommendedIsolationLevel = true,
						UsePageLocksOnDequeue = true,
						DisableGlobalLocks = true
					}));
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

			//GlobalConfiguration.Configuration
			//	.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
			//	.UseColouredConsoleLogProvider()
			//	.UseSimpleAssemblyNameTypeSerializer()
			//	.UseRecommendedSerializerSettings()
			//	.UseSqlServerStorage("Data Source=127.0.0.1,5005;initial catalog=HangfireTest;persist security info=True;User ID=sa;Password=Password1;", new SqlServerStorageOptions
			//	{
			//		CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
			//		SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
			//		QueuePollInterval = TimeSpan.Zero,
			//		UseRecommendedIsolationLevel = true,
			//		UsePageLocksOnDequeue = true,
			//		DisableGlobalLocks = true
			//	});

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();

			//GlobalConfiguration.Configuration
			//	.UseActivator(new HangfireActivator(serviceProvider));
			using var server = new BackgroundJobServer();
			System.Console.ReadLine();
		}
	}

	//public class HangfireActivator : JobActivator
	//{
	//	private readonly IServiceProvider _serviceProvider;

	//	public HangfireActivator(IServiceProvider serviceProvider)
	//	{
	//		_serviceProvider = serviceProvider;
	//	}

	//	public override object ActivateJob(Type type)
	//	{
	//		return _serviceProvider.GetService(type);
	//	}
	//}
}
