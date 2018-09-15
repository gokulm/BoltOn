using System.Collections.Generic;
using System.Reflection;
using BoltOn.IoC;

namespace BoltOn.Bootstrapping
{
    public interface IBootstrapperRegistrationTask
    {
		void Run(IBoltOnContainer container, IEnumerable<Assembly> assemblies);
    }
}
