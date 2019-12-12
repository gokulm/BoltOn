using System.Linq;
using BoltOn.Cqrs;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;
using BoltOn.Other;
using BoltOn.UoW;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bootstrapping
{
	public class BoltOnRegistrationTask : IRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			RegisterByConvention(context);
			RegisterOtherTypes(context);
			RegisterMediator(context);
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
			serviceCollection.AddScoped<IUnitOfWorkManager>(s =>
			{
				return new UnitOfWorkManager(s.GetRequiredService<IBoltOnLogger<UnitOfWorkManager>>(), 
					s.GetRequiredService<IUnitOfWorkFactory>());
			});
			serviceCollection.AddSingleton(typeof(IBoltOnLogger<>), typeof(BoltOnLogger<>));
			serviceCollection.AddSingleton<IBoltOnLoggerFactory, BoltOnLoggerFactory>();
			serviceCollection.AddScoped<EventBag>();

            foreach (var option in Bootstrapper.Instance.OtherOptions)
            {
                serviceCollection.AddSingleton(option.GetType(), option);
            }
		}

		public void RegisterMediator(RegistrationTaskContext context)
		{
			var container = context.Container;
			container.AddTransient<IMediator, Mediator.Pipeline.Mediator>();
			container.AddSingleton<IUnitOfWorkOptionsBuilder, UnitOfWorkOptionsBuilder>();
			RegisterInterceptors(context);
			RegisterHandlers(context);
			RegisterOneWayHandlers(context);
		}

		private static void RegisterInterceptors(RegistrationTaskContext context)
		{
			context.AddInterceptor<StopwatchInterceptor>();

			if (Bootstrapper.Instance.Options.IsCqrsEnabled)
			{
				context.AddInterceptor<CqrsInterceptor>();
				context.Container.AddTransient<IEventDispatcher, EventDispatcher>();
			}
			else
            {
				context.Container.AddTransient<IEventDispatcher, DefaultEventDispatcher>();
			}

			context.AddInterceptor<UnitOfWorkInterceptor>();
		}

		private static void RegisterHandlers(RegistrationTaskContext context)
		{
			var handlerInterfaceType = typeof(IHandler<,>);
			var handlers = (from a in context.Assemblies.ToList()
							from t in a.GetTypes()
							from i in t.GetInterfaces()
							where i.IsGenericType &&
								handlerInterfaceType.IsAssignableFrom(i.GetGenericTypeDefinition())
							select new { Interface = i, Implementation = t }).ToList();
			foreach (var handler in handlers)
				context.Container.AddTransient(handler.Interface, handler.Implementation);
		}

		private static void RegisterOneWayHandlers(RegistrationTaskContext context)
		{
			var handlerInterfaceType = typeof(IHandler<>);
			var handlers = (from a in context.Assemblies.ToList()
							from t in a.GetTypes()
							from i in t.GetInterfaces()
							where i.IsGenericType &&
								handlerInterfaceType.IsAssignableFrom(i.GetGenericTypeDefinition())
							select new { Interface = i, Implementation = t }).ToList();
			foreach (var handler in handlers)
				context.Container.AddTransient(handler.Interface, handler.Implementation);
		}
	}
}
