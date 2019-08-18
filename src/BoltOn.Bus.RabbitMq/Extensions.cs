using System;
using System.Linq;
using System.Reflection;
using BoltOn.Bootstrapping;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bus.RabbitMq
{
	public static partial class Extensions
	{
		public static BoltOnOptions BoltOnRabbitMqBusModule(this BoltOnOptions boltOnOptions)
		{
			boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			return boltOnOptions;
		}

		public static IServiceCollection BoltOnRabbitMqBus(this IServiceCollection serviceCollection, Action<RabbitMqBusOptions> action)
		{
			var options = new RabbitMqBusOptions();
			action(options);

			var busControl = MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
			{
				var host = cfg.Host(new Uri(options.HostAddress), h =>
				{
					h.Username(options.Username);
					h.Password(options.Password);
				});

				var types = from assembly in options.AssembliesWithConsumers
							from type in assembly.GetTypes()
							where typeof(IMessage).IsAssignableFrom(type)
							select type;

				foreach (var type in types)
				{
					var consumer = Activator.CreateInstance(typeof(MassTransitRequestConsumer<>).MakeGenericType(type));	
					// todo (med): support passing queue name convention or name generator in RabbitMqBusOptions									   
					cfg.ReceiveEndpoint(host, $"{type.Name}_queue", endpoint =>
					{
						endpoint.Instance(consumer);
					});
				}
			});

			busControl.Start();
			serviceCollection.AddSingleton(busControl);
			serviceCollection.AddScoped<IBus, BoltOnMassTransitBus>();

			return serviceCollection;
		}
	}
}
