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
		public static IServiceCollection BoltOn(this IServiceCollection serviceCollection, Action<BootstrapperOptions> action = null)
		{
			var options = new BootstrapperOptions(serviceCollection);
			action?.Invoke(options);
			options.RegisterByConvention(Assembly.GetCallingAssembly());
			options.RegisterInterceptors();
			serviceCollection.AddSingleton(options);
			return serviceCollection;
		}

		public static void TightenBolts(this IServiceProvider serviceProvider)
		{
			var bootstrapperOptions = serviceProvider.GetService<BootstrapperOptions>();
			if (!bootstrapperOptions.IsTightened)
			{
				var postRegistrationTasks = serviceProvider.GetServices<IPostRegistrationTask>();
				postRegistrationTasks.ToList().ForEach(t => t.Run());
				bootstrapperOptions.IsTightened = true;
			}
		}

		public static void LoosenBolts(this IServiceProvider serviceProvider)
		{
			var bootstrapperOptions = serviceProvider.GetService<BootstrapperOptions>();
			if (!bootstrapperOptions.IsAppCleaned)
			{
				var postRegistrationTasks = serviceProvider.GetService<IEnumerable<ICleanupTask>>();
				postRegistrationTasks.Reverse().ToList().ForEach(t => t.Run());
				bootstrapperOptions.IsAppCleaned = true;
			}
		}

		public static void After<TInterceptor>(this InterceptorOptions options)
			where TInterceptor : IInterceptor
		{
			var bootstrapperOptions = options.BootstrapperOptions;
			var tempInterceptorTypes = bootstrapperOptions.InterceptorTypes.ToList();
			if (bootstrapperOptions.RecentlyAddedInterceptor != null)
			{
				var recentlyAddedInterceptor = bootstrapperOptions.RecentlyAddedInterceptor;
				var tempIndex = tempInterceptorTypes.IndexOf(typeof(TInterceptor));
				if (tempIndex >= 0)
				{
					tempInterceptorTypes.Remove(recentlyAddedInterceptor);
					if (tempIndex == tempInterceptorTypes.Count)
						tempIndex -= 1;
					tempInterceptorTypes.Insert(tempIndex + 1, recentlyAddedInterceptor);
					bootstrapperOptions.InterceptorTypes = new HashSet<Type>(tempInterceptorTypes);
				}
			}
		}

		public static void Before<TInterceptor>(this InterceptorOptions options)
			where TInterceptor : IInterceptor
		{
			var bootstrapperOptions = options.BootstrapperOptions;
			var tempInterceptorTypes = bootstrapperOptions.InterceptorTypes.ToList();
			if (bootstrapperOptions.RecentlyAddedInterceptor != null)
			{
				var recentlyAddedInterceptor = bootstrapperOptions.RecentlyAddedInterceptor;
				var tempIndex = tempInterceptorTypes.IndexOf(typeof(TInterceptor));
				if (tempIndex >= 0)
				{
					tempInterceptorTypes.Remove(recentlyAddedInterceptor);
					tempInterceptorTypes.Insert(tempIndex, recentlyAddedInterceptor);
					bootstrapperOptions.InterceptorTypes = new HashSet<Type>(tempInterceptorTypes);
				}
			}
		}
	}
}
