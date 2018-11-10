using System.Collections.Generic;
using System.Reflection;
using BoltOn.Bootstrapping;

namespace BoltOn.IoC
{
	public class BoltOnIoCOptions
	{
		private List<Assembly> _assembliesToBeExcluded;
		private List<Assembly> _assembliesToBeIncluded;

		public BoltOnIoCOptions()
		{
			_assembliesToBeExcluded = new List<Assembly>();
			_assembliesToBeIncluded = new List<Assembly>();
		}

		//public IBoltOnContainer Container
		//{
		//	set
		//	{
		//		Bootstrapper.Instance.Container = value;
		//	}
		//}

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
