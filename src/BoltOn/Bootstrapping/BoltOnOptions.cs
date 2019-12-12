using System.Collections.Generic;
using System.Reflection;

namespace BoltOn.Bootstrapping
{
	public sealed class BoltOnOptions
	{
		internal List<Assembly> AssembliesToBeIncluded { get; set; } = new List<Assembly>();

        internal bool IsCqrsEnabled { get; set; }

        public void BoltOnAssemblies(params Assembly[] assemblies)
		{
			AssembliesToBeIncluded.AddRange(assemblies);
		}
	}
}
