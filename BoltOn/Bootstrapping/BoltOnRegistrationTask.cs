using System.Linq;
using BoltOn.IoC;

namespace BoltOn.Bootstrapping
{
	public class BoltOnRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			RegisterByConvention(context);
			RegisterOtherTypes(context.Container);
		}

		private void RegisterByConvention(RegistrationTaskContext context)
		{
			var boltOnIoCOptions = context.GetOptions<BoltOnIoCOptions>();
			var typesToBeExcluded = boltOnIoCOptions.AssemblyOptions.TypesToBeExcluded;
			var interfaces = (from assembly in context.Assemblies
							  from type in assembly.GetTypes()
							  where type.IsInterface
							  select type).ToList();
			var registrations = (from @interface in interfaces
								 from assembly in context.Assemblies
								 from type in assembly.GetTypes()
								 where !type.IsAbstract
									   && type.IsClass && @interface.IsAssignableFrom(type)
									   && type.Name.Equals(@interface.Name.Substring(1))
								       && !typesToBeExcluded.Contains(type)
								 select new { Interface = @interface, Implementation = type }).ToList();

			registrations.ForEach(f => context.Container.RegisterTransient(f.Interface, f.Implementation));
		}

		private void RegisterOtherTypes(IBoltOnContainer container)
		{
			ServiceLocator.SetServiceFactory(container);
			container.RegisterSingleton(typeof(IServiceFactory), new ServiceFactory(container));
			container.RegisterSingleton<IAppContextRetriever, AppContextRetriever>();
		}
	}
}
