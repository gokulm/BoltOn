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
			bootstrapperOptions.ServiceCollection.AddSingleton<IAppServiceBus, AppServiceBus>();
			bootstrapperOptions.ServiceCollection.AddTransient(typeof(AppMessageConsumer<>));
			return bootstrapperOptions;
		}
	}
}
