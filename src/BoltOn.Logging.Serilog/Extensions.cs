using System.Reflection;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BoltOn.Logging.Serilog
{
	public static class Extensions
    {
        public static BoltOnOptions AddSerilogModule(this BoltOnOptions boltOnOptions,
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

            boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
            boltOnOptions.ServiceCollection.AddTransient(typeof(IAppLogger<>),
				typeof(BoltOnSerilogLogger<>));
            return boltOnOptions;
        }
    }
}
