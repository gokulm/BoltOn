using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoltOn.Bootstrapping;
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
				var cleanupTasks = serviceProvider.GetService<IEnumerable<ICleanupTask>>();
				cleanupTasks.Reverse().ToList().ForEach(t => t.Run());
				bootstrapperOptions.IsAppCleaned = true; 
			}
		}
	}
}
