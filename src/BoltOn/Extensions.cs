using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.Cqrs;
using BoltOn.Requestor.Interceptors;
using BoltOn.Transaction;
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
			boltOnOptions.AddInterceptor<CqrsInterceptor>().Before<TransactionInterceptor>();
			boltOnOptions.ServiceCollection.AddSingleton(options);
			boltOnOptions.ServiceCollection.AddTransient<IEventDispatcher, EventDispatcher>();
			return boltOnOptions;
		}

		public static void After<TInterceptor>(this InterceptorOptions options)
			where TInterceptor : IInterceptor
		{
			var boltOnOptions = options.BoltOnOptions;
			var tempInterceptorTypes = boltOnOptions.InterceptorTypes.ToList();
			if (boltOnOptions.RecentlyAddedInterceptor != null)
			{
				var recentlyAddedInterceptor = boltOnOptions.RecentlyAddedInterceptor;
				var tempIndex = tempInterceptorTypes.IndexOf(typeof(TInterceptor));
				if (tempIndex >= 0)
				{
					tempInterceptorTypes.Remove(recentlyAddedInterceptor);
					if (tempIndex == tempInterceptorTypes.Count)
						tempIndex -= 1;
					tempInterceptorTypes.Insert(tempIndex + 1, recentlyAddedInterceptor);
					boltOnOptions.InterceptorTypes = new HashSet<Type>(tempInterceptorTypes);
				}
			}
		}

		public static void Before<TInterceptor>(this InterceptorOptions options)
			where TInterceptor : IInterceptor
		{
			var boltOnOptions = options.BoltOnOptions;
			var tempInterceptorTypes = boltOnOptions.InterceptorTypes.ToList();
			if (boltOnOptions.RecentlyAddedInterceptor != null)
			{
				var recentlyAddedInterceptor = boltOnOptions.RecentlyAddedInterceptor;
				var tempIndex = tempInterceptorTypes.IndexOf(typeof(TInterceptor));
				if (tempIndex >= 0)
				{
					tempInterceptorTypes.Remove(recentlyAddedInterceptor);
					tempInterceptorTypes.Insert(tempIndex, recentlyAddedInterceptor);
					boltOnOptions.InterceptorTypes = new HashSet<Type>(tempInterceptorTypes);
				}
			}
		}
	}
}
