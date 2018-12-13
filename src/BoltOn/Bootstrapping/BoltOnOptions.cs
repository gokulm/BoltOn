using System.Collections.Generic;
using System.Reflection;

namespace BoltOn.Bootstrapping
{
	public sealed class BoltOnOptions
	{
		internal List<Assembly> AssembliesToBeExcluded { get; set; } = new List<Assembly>();

		public void ExcludeAssemblies(params Assembly[] assemblies)
		{
			AssembliesToBeExcluded.AddRange(assemblies);
		}
	}
}
