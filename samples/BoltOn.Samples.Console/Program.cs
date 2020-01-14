using BoltOn.Samples.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Bus.MassTransit;
using Microsoft.Extensions.Logging;
using BoltOn.Data.EF;

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

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
		}
	}
}
