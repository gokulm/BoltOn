using System;
using System.Reflection;
using BoltOn.Bootstrapping;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Hangfire
{
	public static class Extensions
	{
		public static BoltOnOptions BoltOnHangfireModule(this BoltOnOptions boltOnOptions,
			Action<IGlobalConfiguration> configuration = null)
		{
			boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());

			if (configuration != null)
			{
				boltOnOptions.ServiceCollection.AddHangfire((provider, config) => configuration(config));
			}

			return boltOnOptions;
		}

		private static void AddHangfire(
			this IServiceCollection services,
			Action<IServiceProvider, IGlobalConfiguration> configuration)
		{
			services.AddSingleton(serviceProvider =>
			{
				var configurationInstance = GlobalConfiguration.Configuration;
				configuration(serviceProvider, configurationInstance);

				return configurationInstance;
			});
		}
	}
}
