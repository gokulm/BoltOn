using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bootstrapping
{
	public sealed class BoltOnOptions
	{
		internal List<Assembly> AssembliesToBeIncluded { get; set; } = new List<Assembly>();

        internal bool IsCqrsEnabled { get; set; }

		internal IServiceCollection ServiceCollection { get; set; }

		//public Bootstrapper Bootstrapper { get; set; }

		internal List<object> OtherOptions { get; } = new List<object>();

		public void BoltOnAssemblies(params Assembly[] assemblies)
		{
			AssembliesToBeIncluded.AddRange(assemblies);
		}

		public BoltOnOptions(IServiceCollection serviceCollection)
		{
			ServiceCollection = serviceCollection;
			//BootstrapperContainer.Instance.SetOptions(serviceCollection, this);
		}
	}
}
