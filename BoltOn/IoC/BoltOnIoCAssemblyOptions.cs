using System;
using System.Collections.Generic;
using System.Reflection;
using BoltOn.UoW;

namespace BoltOn.IoC
{
    public class BoltOnIoCAssemblyOptions
    {
		public List<Assembly> AssembliesToBeIncluded { get; set; } = new List<Assembly>();
		public List<Assembly> AssembliesToBeExcluded { get; set; } = new List<Assembly>();
		internal List<Type> TypesToBeExcluded { get; set; } = new List<Type>();

		public BoltOnIoCAssemblyOptions()
		{
			AddTypesToBeExcluded(typeof(UnitOfWork), typeof(ContextRetriever), typeof(AppContextRetriever));
		}

		public void AddTypesToBeExcluded(params Type[] types)
		{
			TypesToBeExcluded.AddRange(types);
		}
    }
}
