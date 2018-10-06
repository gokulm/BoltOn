using System.Collections.Generic;
using System.Reflection;

namespace BoltOn.IoC
{
    public class BoltOnIoCAssemblyOptions
    {
        public List<string> AssembliesThatStartWith { get; set; } = new List<string>();
        public List<Assembly> AssembliesToBeExcluded { get; set; } = new List<Assembly>();
    }
}
