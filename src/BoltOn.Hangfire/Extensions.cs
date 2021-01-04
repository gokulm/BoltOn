using System.Reflection;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Hangfire
{
	public static class Extensions
	{
		public static BootstrapperOptions BoltOnHangfireModule(this BootstrapperOptions bootstrapperOptions)
		{
			bootstrapperOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			bootstrapperOptions.ServiceCollection.AddTransient<AppHangfireJobProcessor>();
			return bootstrapperOptions;
		}
	}
}
