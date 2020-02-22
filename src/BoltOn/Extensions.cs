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
			options.RegisterInterceptors();
			serviceCollection.AddSingleton(options);
			return serviceCollection;
		}

		public static void TightenBolts(this IServiceProvider serviceProvider)
		{
			var boltOnOptions = serviceProvider.GetService<BoltOnOptions>();
            if (!boltOnOptions.IsTightened)
            {
                var postRegistrationTasks = serviceProvider.GetServices<IPostRegistrationTask>();
				postRegistrationTasks.ToList().ForEach(t => t.Run());
                boltOnOptions.IsTightened = true;
            }
        }

		public static void LoosenBolts(this IServiceProvider serviceProvider)
		{
            var boltOnOptions = serviceProvider.GetService<BoltOnOptions>();
            if (!boltOnOptions.IsAppCleaned)
			{
                var postRegistrationTasks = serviceProvider.GetService<IEnumerable<ICleanupTask>>();
                postRegistrationTasks.Reverse().ToList().ForEach(t => t.Run());
                boltOnOptions.IsAppCleaned = true;
            }
		}

		public static BoltOnOptions BoltOnCqrsModule(this BoltOnOptions boltOnOptions,
			Action<CqrsOptions> action = null)
        {
			boltOnOptions.IsCqrsEnabled = true;
			var options = new CqrsOptions();
			action?.Invoke(options);
			boltOnOptions.ServiceCollection.AddSingleton(options);
			boltOnOptions.ServiceCollection.AddTransient<IEventDispatcher, EventDispatcher>();
			return boltOnOptions;
		}
    }
}
