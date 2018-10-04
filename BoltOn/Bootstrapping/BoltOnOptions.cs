using System.Collections.Generic;
using System.Reflection;
using BoltOn.IoC;

namespace BoltOn.Bootstrapping
{
	public class BoltOnOptions
	{
		public List<Assembly> AssembliesToBeExcluded { get; set; } = new List<Assembly>();
		public IBoltOnContainer Container { get; set; }
		public List<Assembly> Assemblies { get; set; } = new List<Assembly>();
		public Assembly CallingAssembly { get; internal set; }

	}
}
