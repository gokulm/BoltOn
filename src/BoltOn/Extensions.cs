using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.Cqrs;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn
{
	public static class Extensions
	{
		public static IServiceCollection BoltOn(this IServiceCollection serviceCollection, Action<BoltOnOptions> action = null)
		{
			var options = new BoltOnOptions(serviceCollection);
			action?.Invoke(options);
			options.RegisterByConvention(Assembly.GetCallingAssembly());
			var bootstrapper = Bootstrapper.Create(options, Assembly.GetCallingAssembly());
            serviceCollection.AddSingleton(bootstrapper);
			return serviceCollection;
		}

		public static void TightenBolts(this IServiceProvider serviceProvider)
		{
			var bootstrapper = serviceProvider.GetService<Bootstrapper>();
            if (!bootstrapper.IsTightened)
            {
                var postRegistrationTasks = serviceProvider.GetServices<IPostRegistrationTask>();
				postRegistrationTasks.ToList().ForEach(t => t.Run());
                bootstrapper.IsTightened = true;
            }
        }

		public static void LoosenBolts(this IServiceProvider serviceProvider)
		{
            var bootstrapper = serviceProvider.GetService<Bootstrapper>();
            if (!bootstrapper.IsAppCleaned)
			{
                var postRegistrationTasks = serviceProvider.GetService<IEnumerable<ICleanupTask>>();
                postRegistrationTasks.Reverse().ToList().ForEach(t => t.Run());
                bootstrapper.IsAppCleaned = true;
            }
			bootstrapper.Dispose();
		}

		public static BoltOnOptions BoltOnCqrsModule(this BoltOnOptions boltOnOptions,
			Action<CqrsOptions> action = null)
        {
			boltOnOptions.IsCqrsEnabled = true;
			var options = new CqrsOptions();
			action?.Invoke(options);
			boltOnOptions.ServiceCollection.AddSingleton(options);
			boltOnOptions.ServiceCollection.AddTransient<IEventDispatcher, EventBusDispatcher>();
			return boltOnOptions;
		}
    }
}
