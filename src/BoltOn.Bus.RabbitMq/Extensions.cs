using System;
using System.Reflection;
using BoltOn.Bootstrapping;
using MassTransit;

namespace BoltOn.Bus.RabbitMq
{
	public static class Extensions
	{
		public static BoltOnOptions BoltOnRabbitMqBus(this BoltOnOptions boltOnOptions, Action<RabbitMqBusOptions> action)
		{
			boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());

			var options = new RabbitMqBusOptions();
			action(options);

			//boltOnOptions.Set(options);

			var busControl = MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
			{
				cfg.Host(new Uri(options.HostAddress), h =>
				{
					h.Username(options.Username);
					h.Password(options.Password);
				});
			});
			busControl.Start();

			return boltOnOptions;
		}
	}

	public class RegistrationTask : IRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			throw new NotImplementedException();
		}
	}
}
