using System.Linq;
using BoltOn.Context;
using BoltOn.IoC;
using BoltOn.Other;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bootstrapping
{
	public class BoltOnRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			RegisterByConvention(context);
			RegisterOtherTypes(context);
		}

		private void RegisterByConvention(RegistrationTaskContext context)
		{
			var boltOnIoCOptions = context.GetOptions<BoltOnIoCOptions>();
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
								 && !type.GetCustomAttributes(typeof(ExcludeFromRegistrationAttribute), true).Any()
								 select new { Interface = @interface, Implementation = type }).ToList();

			registrations.ForEach(f => context.ServiceCollection.AddTransient(f.Interface, f.Implementation));

		}

		private void RegisterOtherTypes(RegistrationTaskContext context)
		{
			var serviceCollection = context.ServiceCollection;
			serviceCollection.AddSingleton<IAppContextRetriever, AppContextRetriever>();
			serviceCollection.AddScoped<IContextRetriever>((serviceProvider) => 
			                                            new ContextRetriever(serviceProvider.GetService(typeof(IAppContextRetriever)) as IAppContextRetriever));
		}
	}
} 
