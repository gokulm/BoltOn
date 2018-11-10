using BoltOn.Bootstrapping;
using BoltOn.Tests.Common;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Bootstrapping
{
	public class TestBootstrapperRegistrationTask : IBootstrapperRegistrationTask
    {
		public void Run(RegistrationTaskContext context)
        {
			context.ServiceCollection
			       .AddTransient<Employee>()
			       .AddTransient<ClassWithInjectedDependency>();
        }
    }
}
