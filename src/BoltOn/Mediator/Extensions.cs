using System.Linq;
using BoltOn.Mediator.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Mediator
{
	public static class Extensions
	{
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
