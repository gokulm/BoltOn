using System.Reflection;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Hangfire
{
	public static class Extensions
	{
		public static BoltOnOptions BoltOnHangfireModule(this BoltOnOptions boltOnOptions)
		{
			boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			boltOnOptions.ServiceCollection.AddTransient<BoltOnHangfireJobProcessor>();
			return boltOnOptions;
		}
	}
}
