using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoltOn.IoC;

namespace BoltOn.Bootstrapping
{
    public class BoltOnRegistrationTask : IBootstrapperRegistrationTask
    {
		public void Run(RegistrationTaskContext context)
		{
			RegisterByConvention(context.Container, context.Assemblies);
			RegisterOtherTypes(context.Container);
        }

        private void RegisterByConvention(IBoltOnContainer container, IEnumerable<Assembly> assemblies)
        {
            var interfaces = (from assembly in assemblies
			                  from type in assembly.GetTypes()
			                  where type.IsInterface
                              select type).ToList();
            var registrations = (from @interface in interfaces
                                 from assembly in assemblies
                                 from type in assembly.GetTypes()
                                 where !type.IsAbstract
                                       && type.IsClass && @interface.IsAssignableFrom(type)
                                       && type.Name.Equals(@interface.Name.Substring(1))
                                 select new { Interface = @interface, Implementation = type }).ToList();

			registrations.ForEach(f => container.RegisterTransient(f.Interface, f.Implementation));
        }

		private void RegisterOtherTypes(IBoltOnContainer container)
		{
			ServiceLocator.SetServiceFactory(container);
			container.RegisterSingleton(typeof(IServiceFactory), new ServiceFactory(container));
		}
    }
}
