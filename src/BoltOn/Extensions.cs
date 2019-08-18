using System;
using System.Linq;
using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.Mediator.Interceptors;
using BoltOn.Other;
using Microsoft.Extensions.DependencyInjection;

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
			BoltOnServiceProvider.Current = serviceProvider;
			Bootstrapper.Instance.RunPostRegistrationTasks(serviceProvider);
		}

		internal static IServiceCollection AddInterceptor(this IServiceCollection services, Type interceptorType)
		{
			var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == interceptorType);
			if (serviceDescriptor == null)
				services.AddTransient(typeof(IInterceptor), interceptorType);
			return services;
		}

		public static void RegisterByConvention(this IServiceCollection services, Assembly assembly)
		{
			var interfaces = (from type in assembly.GetTypes()
							  where type.IsInterface
							  select type).ToList();
			var tempRegistrations = (from @interface in interfaces
									 from type in assembly.GetTypes()
									 where !type.IsAbstract
										   && type.IsClass && @interface.IsAssignableFrom(type)
									 && !type.GetCustomAttributes(typeof(ExcludeFromRegistrationAttribute), true).Any()
									 select new { Interface = @interface, Implementation = type }).ToList();

			// get interfaces with only one implementation
			var registrations = (from r in tempRegistrations
								 group r by r.Interface into grp
								 where grp.Count() == 1
								 select new { Interface = grp.Key, grp.First().Implementation }).ToList();

			registrations.ForEach(f => services.AddTransient(f.Interface, f.Implementation));
		}
	}
}
