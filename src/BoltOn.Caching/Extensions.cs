using System.Reflection;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Caching
{
	public static class Extensions
	{
		public static BoltOnOptions BoltOnCaching(this BoltOnOptions boltOnOptions)
		{
			boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			boltOnOptions.ServiceCollection.AddTransient<IBoltOnCache, BoltOnCache>();
			return boltOnOptions;
		}
	}
}
