using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace BoltOn.Bootstrapping
{
	public sealed class BoltOnOptions
	{
		internal Hashtable AppOptions { get; set; } = new Hashtable();

		internal List<Assembly> AssembliesToBeIncluded { get; set; } = new List<Assembly>();

		public void BoltOnAssemblies(params Assembly[] assemblies)
		{
			AssembliesToBeIncluded.AddRange(assemblies);
		}

		public void Set<TOptions>(TOptions options) where TOptions : class
		{
			var key = typeof(TOptions).FullName;
			if (AppOptions.ContainsKey(key))
				AppOptions.Add(key, options);
		}
	}
}
