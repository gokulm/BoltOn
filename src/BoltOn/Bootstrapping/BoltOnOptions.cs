using System.Collections.Generic;
using System.Reflection;

namespace BoltOn.Bootstrapping
{
	public sealed class BoltOnOptions
	{
		internal List<Assembly> AssembliesToBeIncluded { get; set; } = new List<Assembly>();
		internal List<Assembly> AssembliesToBeExcluded { get; set; } = new List<Assembly>();

		public void IncludeAssemblies(params Assembly[] assemblies)
		{
			AssembliesToBeIncluded.AddRange(assemblies);
		}

		public void ExcludeAssemblies(params Assembly[] assemblies)
		{
			AssembliesToBeExcluded.AddRange(assemblies);
		}
	}
}
