using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Bus.RabbitMq;

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

			serviceCollection.BoltOnRabbitMqBus(o =>
			{
				o.HostAddress = "rabbitmq://localhost:5672";
				o.Username = "guest";
				o.Password = "guest";
				o.AssembliesWithConsumers.Add(typeof(CreateStudentHandler).Assembly);
			});

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();

			var mediator = serviceProvider.GetRequiredService<IMediator>();
			var response = mediator.Process(new PingRequest());
			System.Console.WriteLine(response.Data);
		}
	}
}
