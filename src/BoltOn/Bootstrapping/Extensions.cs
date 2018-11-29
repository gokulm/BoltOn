using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bootstrapping
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
			Bootstrapper.Instance.RunPostRegistrationTasks(serviceProvider);
		}
	}
}
