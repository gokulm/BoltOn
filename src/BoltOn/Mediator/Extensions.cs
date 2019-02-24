using System.Linq;
using BoltOn.Mediator.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Mediator
{
	public static class Extensions
    {
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
