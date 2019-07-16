using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BoltOn.Utilities;

namespace BoltOn.Bootstrapping
{
	public sealed class BoltOnOptions
	{
		private readonly Hashtable _options = new Hashtable();

		internal List<Assembly> AssembliesToBeIncluded { get; set; } = new List<Assembly>();

		public void BoltOnAssemblies(params Assembly[] assemblies)
		{
			AssembliesToBeIncluded.AddRange(assemblies);
		}

		//public void Set<TOptions>(TOptions options) where TOptions : class
		//{
		//	var key = typeof(TOptions).FullName;
		//	if (_options.ContainsKey(key))
		//		_options.Add(key, options);
		//}

		//public TOptions Get<TOptions>() where TOptions : class
		//{
		//	var key = typeof(TOptions).FullName;
		//	Check.Requires(_options.ContainsKey(key), "Options not found");
		//	return (TOptions)_options[key];
		//}
	}
}
