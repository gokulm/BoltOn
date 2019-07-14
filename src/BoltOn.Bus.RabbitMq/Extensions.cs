using System;
using System.Reflection;
using BoltOn.Bootstrapping;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bus.RabbitMq
{
	public static class Extensions
	{
		public static BoltOnOptions BoltOnRabbitMqBus(this BoltOnOptions boltOnOptions, Action<RabbitMqBusOptions> action)
		{
			var serviceCollection = boltOnOptions.ServiceCollection;
			boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());

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

			return boltOnOptions;
		}
	}
}
