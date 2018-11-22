using System;
using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.IoC
{
	public static class Extensions
    {
		public static IServiceCollection BoltOn(this IServiceCollection serviceCollection, Action<BoltOnIoCOptions> action = null)
		{
			Check.Requires(!Bootstrapper.Instance.IsBolted, "Components are already bolted! IoC cannot be configured now");
			var options = new BoltOnIoCOptions();
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
