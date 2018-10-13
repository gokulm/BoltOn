using System;
using System.Collections.Generic;
using System.Reflection;

namespace BoltOn.IoC
{
    public class BoltOnIoCAssemblyOptions
    {
		public List<Assembly> AssembliesToBeIncluded { get; set; } = new List<Assembly>();
        public List<Assembly> AssembliesToBeExcluded { get; set; } = new List<Assembly>();
    }
}
