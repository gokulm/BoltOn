using System.Collections.Generic;
using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.IoC;
using BoltOn.Tests.Common;

namespace BoltOn.Tests.Bootstrapping
{
    public class TestBootstrapperRegistrationTask : IBootstrapperRegistrationTask
    {
		public void Run(RegistrationTaskContext context)
        {
			context.Container.RegisterTransient<Employee>()
                     .RegisterTransient<ClassWithInjectedDependency>();
        }
    }
}
