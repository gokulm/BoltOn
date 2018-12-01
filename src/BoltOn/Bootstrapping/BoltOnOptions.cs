using System.Collections.Generic;
using System.Reflection;

namespace BoltOn.Bootstrapping
{
	public sealed class BoltOnOptions
	{
		private List<Assembly> _assembliesToBeExcluded;
		private List<Assembly> _assembliesToBeIncluded;

		public BoltOnOptions()
		{
			_assembliesToBeExcluded = new List<Assembly>();
			_assembliesToBeIncluded = new List<Assembly>();
		}

		internal IReadOnlyList<Assembly> AssembliesToBeIncluded => _assembliesToBeIncluded.AsReadOnly();
		internal IReadOnlyList<Assembly> AssembliesToBeExcluded => _assembliesToBeExcluded.AsReadOnly();

		public void IncludeAssemblies(params Assembly[] assemblies)
		{
			_assembliesToBeIncluded.AddRange(assemblies);
		}

		public void ExcludeAssemblies(params Assembly[] assemblies)
		{
			_assembliesToBeExcluded.AddRange(assemblies);
		}
	}
}
