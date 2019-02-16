using System;
using System.Linq;
using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.Mediator.Middlewares;
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

		public static void UseBoltOn(this IServiceProvider serviceProvider)
		{
			var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
			Check.Requires(loggerFactory != null, "Add logging to the service collection");
			Bootstrapper.Instance.RunPostRegistrationTasks(serviceProvider);
		}

		public static IServiceCollection RemoveAllMiddlewares(this IServiceCollection services)
		{
			var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IMediatorMiddleware));
			if (serviceDescriptor != null)
				services.Remove(serviceDescriptor);
			return services;
		}

		public static IServiceCollection AddMiddleware<TMiddleware>(this IServiceCollection services)
		{
			services.AddTransient(typeof(IMediatorMiddleware), typeof(TMiddleware));
			return services;
		}
	}
}
