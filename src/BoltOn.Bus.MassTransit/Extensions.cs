using System.Reflection;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bus.MassTransit
{
	public static class Extensions
	{
		public static BoltOnOptions BoltOnMassTransitBusModule(this BoltOnOptions boltOnOptions)
		{
			boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
            boltOnOptions.ServiceCollection.AddSingleton<IBus, BoltOnMassTransitBus>();
            boltOnOptions.ServiceCollection.AddTransient(typeof(BoltOnMassTransitConsumer<>));
			return boltOnOptions;
		}
	}
}
