using System.Reflection;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BoltOn.Logging.Serilog
{
	public static class Extensions
	{
		public static BootstrapperOptions BoltOnSerilogModule(this BootstrapperOptions bootstrapperOptions,
			IConfiguration configuration = null)
		{
			if (configuration != null)
			{
				Log.Logger = new LoggerConfiguration()
								.Enrich.WithMachineName()
								.Enrich.FromLogContext()
								.ReadFrom.Configuration(configuration)
								.CreateLogger();
			}

			bootstrapperOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			bootstrapperOptions.ServiceCollection.AddTransient(typeof(IAppLogger<>),
				typeof(AppSerilogLogger<>));
			return bootstrapperOptions;
		}
	}
}
