using System;
using System.Reflection;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn
{
	public static class Extensions
	{
		public static IServiceCollection BoltOn(this IServiceCollection serviceCollection, Action<BoltOnOptions> action = null)
		{
			var options = new BoltOnOptions(serviceCollection);
			action?.Invoke(options);
			_ = Bootstrapper.Create(options, Assembly.GetCallingAssembly());
			return serviceCollection;
		}

		public static void TightenBolts(this IServiceProvider serviceProvider)
		{
			var bootstrapper = serviceProvider.GetService<Bootstrapper>();
			bootstrapper.RunPostRegistrationTasks(serviceProvider);
		}

		public static void LoosenBolts(this IServiceProvider serviceProvider)
		{
			var bootstrapper = serviceProvider.GetService<Bootstrapper>();
			bootstrapper.Dispose();
		}

		public static BoltOnOptions BoltOnCqrsModule(this BoltOnOptions boltOnOptions,
			Action<CqrsOptions> action = null)
        {
			boltOnOptions.IsCqrsEnabled = true;
			var options = new CqrsOptions();
			action?.Invoke(options);
			boltOnOptions.ServiceCollection.AddSingleton(options);
			return boltOnOptions;
		}
	}
}
