using System.Reflection;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bus.MassTransit
{
	public static class Extensions
	{
		public static BootstrapperOptions BoltOnMassTransitBusModule(this BootstrapperOptions bootstrapperOptions)
		{
			bootstrapperOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			bootstrapperOptions.ServiceCollection.AddSingleton<IBus, BoltOnMassTransitBus>();
			bootstrapperOptions.ServiceCollection.AddTransient(typeof(BoltOnMassTransitConsumer<>));
			return bootstrapperOptions;
		}
	}
}
