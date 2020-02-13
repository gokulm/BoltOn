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
        public void Run(BoltOnOptions boltOnOptions)
        {
			//boltOnOptions.BoltOnAssemblies(GetType().Assembly);
            RegisterOtherTypes(boltOnOptions.ServiceCollection);
            RegisterMediator(boltOnOptions);
        }

		public void Run(RegistrationTaskContext context)
		{
			//RegisterByConvention(context);
			//RegisterOtherTypes(context);
			//RegisterMediator(context);
		}

		private void RegisterOtherTypes(IServiceCollection serviceCollection)
		{
			serviceCollection.AddScoped<IUnitOfWorkManager>(s =>
			{
				return new UnitOfWorkManager(s.GetRequiredService<IBoltOnLogger<UnitOfWorkManager>>(), 
					s.GetRequiredService<IUnitOfWorkFactory>());
			});
			serviceCollection.AddSingleton(typeof(IBoltOnLogger<>), typeof(BoltOnLogger<>));
			serviceCollection.AddSingleton<IBoltOnLoggerFactory, BoltOnLoggerFactory>();
			serviceCollection.AddScoped<EventBag>();

            //foreach (var option in context.Bootstrapper.Options.OtherOptions)
            //{
            //    serviceCollection.AddSingleton(option.GetType(), option);
            //}
		}

		public void RegisterMediator(BoltOnOptions boltOnOptions)
		{
			boltOnOptions.ServiceCollection.AddTransient<IMediator, Mediator.Pipeline.Mediator>();
			boltOnOptions.ServiceCollection.AddSingleton<IUnitOfWorkOptionsBuilder, UnitOfWorkOptionsBuilder>();
			boltOnOptions.AddInterceptor<StopwatchInterceptor>();
			boltOnOptions.AddInterceptor<UnitOfWorkInterceptor>();

		}

		private static void RegisterInterceptors(IServiceCollection serviceCollection)
		{
			//context.AddInterceptor<StopwatchInterceptor>();

			//if (context.Bootstrapper.Options.IsCqrsEnabled)
			//{
			//	context.AddInterceptor<CqrsInterceptor>();
			//	context.ServiceCollection.AddTransient<IEventDispatcher, EventDispatcher>();
			//}

			//context.AddInterceptor<UnitOfWorkInterceptor>();
		}

		//private static void RegisterHandlers(BoltOnOptions boltOnOptions)
		//{
		//	var handlerInterfaceType = typeof(IHandler<,>);
		//	var handlers = (from a in boltOnOptions.AssembliesToBeIncluded
		//					from t in a.GetTypes()
		//					from i in t.GetInterfaces()
		//					where i.IsGenericType &&
		//						handlerInterfaceType.IsAssignableFrom(i.GetGenericTypeDefinition())
		//					select new { Interface = i, Implementation = t }).ToList();
		//	foreach (var handler in handlers)
		//		boltOnOptions.ServiceCollection.AddTransient(handler.Interface, handler.Implementation);
		//}

		//private static void RegisterOneWayHandlers(BoltOnOptions boltOnOptions)
		//{
		//	var handlerInterfaceType = typeof(IHandler<>);
		//	var handlers = (from a in boltOnOptions.AssembliesToBeIncluded.ToList()
		//					from t in a.GetTypes()
		//					from i in t.GetInterfaces()
		//					where i.IsGenericType &&
		//						handlerInterfaceType.IsAssignableFrom(i.GetGenericTypeDefinition())
		//					select new { Interface = i, Implementation = t }).ToList();
		//	foreach (var handler in handlers)
		//		boltOnOptions.ServiceCollection.AddTransient(handler.Interface, handler.Implementation);
		//}
	}
}
