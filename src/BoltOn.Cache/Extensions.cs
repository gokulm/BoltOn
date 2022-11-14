using System.Reflection;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Cache
{
	public static class Extensions
	{
		public static BootstrapperOptions BoltOnCacheModule(this BootstrapperOptions bootstrapperOptions)
		{
			bootstrapperOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			bootstrapperOptions.ServiceCollection.AddTransient<IAppCache, AppCache>();
			return bootstrapperOptions;
		}
	}
}
