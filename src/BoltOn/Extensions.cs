using System;
using System.Linq;
using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.Mediator.Interceptors;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BoltOn
{
	public static class Extensions
    {
		public static IServiceCollection BoltOn(this IServiceCollection serviceCollection, Action<BoltOnOptions> action = null)
		{
			var options = new BoltOnOptions();
			action?.Invoke(options);
			Bootstrapper.Instance.BoltOn(serviceCollection, options, Assembly.GetCallingAssembly());
			return serviceCollection;
		}

		public static void TightenBolts(this IServiceProvider serviceProvider)
		{
			var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
			Check.Requires(loggerFactory != null, "Add logging to the service collection");
			Bootstrapper.Instance.RunPostRegistrationTasks(serviceProvider);
		}

		public static IServiceCollection RemoveAllInterceptors(this IServiceCollection services)
		{
			var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IInterceptor));
			if (serviceDescriptor != null)
				services.Remove(serviceDescriptor);
			return services;
		}

		public static IServiceCollection AddInterceptor<TInterceptor>(this IServiceCollection services)
		{
			services.AddTransient(typeof(IInterceptor), typeof(TInterceptor));
			return services;
		}

		public static IServiceCollection RemoveInterceptor<TInterceptor>(this IServiceCollection services)
		{
			var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ImplementationType == typeof(TInterceptor));
			if (serviceDescriptor != null)
				services.Remove(serviceDescriptor);
			return services;
		}
	}
}
