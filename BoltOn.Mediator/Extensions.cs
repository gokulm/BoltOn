using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Mediator
{
	public static class Extensions
    {
		public static IServiceCollection ClearMiddlewares(this IServiceCollection services)
		{
			var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IMediatorMiddleware));
			if (serviceDescriptor != null) 
				services.Remove(serviceDescriptor);
			return services;
		}

		public static IServiceCollection RegisterMiddleware<TMiddleware>(this IServiceCollection services)
		{
			services.AddTransient(typeof(IMediatorMiddleware), typeof(TMiddleware));
			return services;
		}
	}
}
