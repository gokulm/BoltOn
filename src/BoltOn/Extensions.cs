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
			Bootstrapper.Instance.RunPostRegistrationTasks(serviceProvider);
		}

		internal static IServiceCollection AddInterceptor(this IServiceCollection services, Type interceptorType)
		{
			var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == interceptorType);
			if (serviceDescriptor == null)
				services.AddTransient(typeof(IInterceptor), interceptorType);
			return services;
		}

		public static void AddInterceptor<TInterceptor>(this RegistrationTaskContext context) where TInterceptor : IInterceptor
		{
			context.AddInterceptor<TInterceptor>();
		}

		public static void RemoveInterceptor<TInterceptor>(this RegistrationTaskContext context) where TInterceptor : IInterceptor
		{
			context.RemoveInterceptor<TInterceptor>();
		}

		public static void RemoveAllInterceptors(this RegistrationTaskContext context)
		{
			context.RemoveAllInterceptors();
		}
	}
}
