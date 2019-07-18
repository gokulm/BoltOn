using System;
using System.Reflection;
using BoltOn.Bootstrapping;

namespace BoltOn.Bus.RabbitMq
{
	public static class Extensions
	{
		public static BoltOnOptions BoltOnRabbitMqBusModule(this BoltOnOptions boltOnOptions, Action<RabbitMqBusOptions> action)
		{
			boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			var options = new RabbitMqBusOptions();
			action(options);
			boltOnOptions.Set(options);

			return boltOnOptions;
		}
	}
}
