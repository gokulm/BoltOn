using System.Reflection;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Cache
{
	public static class Extensions
	{
		public static BoltOnOptions BoltOnCacheModule(this BoltOnOptions boltOnOptions)
		{
			boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			boltOnOptions.ServiceCollection.AddTransient<IBoltOnCache, BoltOnCache>();
			return boltOnOptions;
		}
	}
}
