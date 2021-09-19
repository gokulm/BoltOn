using System;
using System.Linq;
using System.Reflection;
using BoltOn.Bootstrapping;
using Marten;

namespace BoltOn.Data.MartenDb
{
	public static class Extensions
	{
		public static BootstrapperOptions BoltOnMartenDbModule(this BootstrapperOptions bootstrapperOptions,
			StoreOptions storeOptions)
		{
			bootstrapperOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			bootstrapperOptions.ServiceCollection.AddMarten(storeOptions);
			return bootstrapperOptions;
		}
	}
}
