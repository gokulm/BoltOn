using System.Collections.Generic;
using System.Reflection;

namespace BoltOn.Bootstrapping
{
	public sealed class BoltOnOptions
	{
		internal List<Assembly> AssembliesToBeIncluded { get; set; } = new List<Assembly>();

		public void AddAssemblies(params Assembly[] assemblies)
		{
			AssembliesToBeIncluded.AddRange(assemblies);
		}
	}
}
