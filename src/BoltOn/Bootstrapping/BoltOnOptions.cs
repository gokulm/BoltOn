using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bootstrapping
{
	public sealed class BoltOnOptions
	{
		internal List<Assembly> AssembliesToBeIncluded { get; set; } = new List<Assembly>();

        internal bool IsCqrsEnabled { get; set; }

		internal List<object> OtherOptions { get; } = new List<object>();

		public IServiceCollection ServiceCollection { get; private set; }

		public BoltOnOptions(IServiceCollection serviceCollection)
		{
			ServiceCollection = serviceCollection;
		}

		public void BoltOnAssemblies(params Assembly[] assemblies)
		{
			AssembliesToBeIncluded.AddRange(assemblies);
		}
	}
}
