using System;
using System.Reflection;
using BoltOn.Bootstrapping;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bus.RabbitMq
{
	public static class Extensions
	{
		public static BoltOnOptions BoltOnRabbitMqBus(this BoltOnOptions boltOnOptions)
        {
			boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			return boltOnOptions;
		}

        public static IServiceCollection AddRabbitMqBus(this IServiceCollection serviceCollection, Action<RabbitMqBusOptions> action)
        {
            var options = new RabbitMqBusOptions();
            action(options);

            var busControl = MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri(options.HostAddress), h =>
                {
                    h.Username(options.Username);
                    h.Password(options.Password);
                });
            });
            busControl.Start();
            serviceCollection.AddSingleton(busControl);

            return serviceCollection;
        }
    }
}
