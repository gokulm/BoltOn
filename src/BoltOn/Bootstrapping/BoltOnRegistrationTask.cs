using System.Linq;
using BoltOn.Logging;
using BoltOn.Other;
using BoltOn.UoW;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bootstrapping
{
	internal class BoltOnRegistrationTask : IRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			RegisterByConvention(context);
			RegisterOtherTypes(context);
		}

		private void RegisterByConvention(RegistrationTaskContext context)
		{
			var interfaces = (from assembly in context.Assemblies
							  from type in assembly.GetTypes()
							  where type.IsInterface
							  select type).ToList();
			var tempRegistrations = (from @interface in interfaces
									 from assembly in context.Assemblies
									 from type in assembly.GetTypes()
									 where !type.IsAbstract
										   && type.IsClass && @interface.IsAssignableFrom(type)
									 && !type.GetCustomAttributes(typeof(ExcludeFromRegistrationAttribute), true).Any()
									 select new { Interface = @interface, Implementation = type }).ToList();

			// get interfaces with only one implementation
			var registrations = (from r in tempRegistrations
								 group r by r.Interface into grp
								 where grp.Count() == 1
								 select new { Interface = grp.Key, grp.First().Implementation }).ToList();

			registrations.ForEach(f => context.Container.AddTransient(f.Interface, f.Implementation));
		}

		private void RegisterOtherTypes(RegistrationTaskContext context)
		{
			var serviceCollection = context.Container;
			serviceCollection.AddScoped<IUnitOfWorkManager, UnitOfWorkManager>();
			serviceCollection.AddScoped<IUnitOfWorkManager>(s => 
			    new UnitOfWorkManager(s.GetRequiredService<IBoltOnLogger<UnitOfWorkManager>>(), 
			    s.GetRequiredService<IUnitOfWorkFactory>()));
			serviceCollection.AddSingleton(typeof(IBoltOnLogger<>), typeof(BoltOnLogger<>));
			serviceCollection.AddSingleton<IBoltOnLoggerFactory, BoltOnLoggerFactory>();
		}
	}
}
