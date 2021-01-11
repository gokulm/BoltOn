using System;
using System.IO;
using BoltOn.Data.EF;
using BoltOn.Samples.Application.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
			});

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();  
			System.Console.ReadLine();
		}
	}
}
