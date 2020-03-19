using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoltOn.Cqrs;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;
using BoltOn.Other;
using BoltOn.UoW;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bootstrapping
{
	public sealed class BoltOnOptions
	{
		internal bool IsCqrsEnabled { get; set; }
		internal bool IsTightened { get; set; }
		internal bool IsAppCleaned { get; set; }
		internal HashSet<Assembly> RegisteredAssemblies { get; } = new HashSet<Assembly>();
		public IServiceCollection ServiceCollection { get; }
        internal HashSet<Type> InterceptorTypes { get; set; } = new HashSet<Type>();
        internal Type RecentlyAddedInterceptor { get; set; }
        internal InterceptorOptions InterceptorOptions { get; }

		public BoltOnOptions(IServiceCollection serviceCollection)
		{
			ServiceCollection = serviceCollection;
			InterceptorOptions = new InterceptorOptions(this);
			RegisterByConvention(GetType().Assembly);
			RegisterCoreTypes();
			RegisterMediator();
		}

		public void BoltOnAssemblies(params Assembly[] assemblies)
		{
			RegisterByConvention(assemblies);
		}

		private void RegisterCoreTypes()
		{
			ServiceCollection.AddScoped<IUnitOfWorkManager>(s => 
                new UnitOfWorkManager(s.GetRequiredService<IBoltOnLogger<UnitOfWorkManager>>(),
                s.GetRequiredService<IUnitOfWorkFactory>()));
			ServiceCollection.AddSingleton(typeof(IBoltOnLogger<>), typeof(BoltOnLogger<>));
			ServiceCollection.AddSingleton<IBoltOnLoggerFactory, BoltOnLoggerFactory>();
			ServiceCollection.AddTransient<IEventDispatcher, DefaultEventDispatcher>();
			ServiceCollection.AddScoped<EventBag>();
			var options = new CqrsOptions();
			ServiceCollection.AddSingleton(options);
		}

		private void RegisterMediator()
		{
			ServiceCollection.AddTransient<IMediator, Mediator.Pipeline.Mediator>();
			ServiceCollection.AddSingleton<IUnitOfWorkOptionsBuilder, UnitOfWorkOptionsBuilder>();
			AddInterceptor<StopwatchInterceptor>();
			AddInterceptor<UnitOfWorkInterceptor>();
		}

		public InterceptorOptions AddInterceptor<TInterceptor>() where TInterceptor : IInterceptor
		{
			InterceptorTypes.Add(typeof(TInterceptor));
            RecentlyAddedInterceptor = typeof(TInterceptor);
			return new InterceptorOptions(this);
		}

		public InterceptorOptions RemoveInterceptor<TInterceptor>() where TInterceptor : IInterceptor
		{
			InterceptorTypes.Remove(typeof(TInterceptor));
            return new InterceptorOptions(this);
		}

		public void RemoveAllInterceptors()
		{
			InterceptorTypes.Clear();
		}

		internal void RegisterByConvention(Assembly assembly)
		{
			RegisterByConvention(new List<Assembly> { assembly });
		}

		internal void RegisterInterceptors()
		{
			foreach (var interceptorImplementation in InterceptorTypes)
			{
				var serviceDescriptor = ServiceCollection.FirstOrDefault(descriptor =>
							descriptor.ServiceType == interceptorImplementation);
				if (serviceDescriptor == null)
					ServiceCollection.AddTransient(typeof(IInterceptor), interceptorImplementation);
			}
		}

		private void RegisterByConvention(IEnumerable<Assembly> assemblies)
		{
			foreach (var assembly in assemblies.ToList())
			{
                if (RegisteredAssemblies.Contains(assembly)) 
                    continue;

                var interfaces = (from type in assembly.GetTypes()
                    where type.IsInterface
                    select type).ToList();
                var tempRegistrations = (from @interface in interfaces
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

                registrations.ForEach(f => ServiceCollection.AddTransient(f.Interface, f.Implementation));

                RegisterHandlers(assembly);
                RegisterOneWayHandlers(assembly);
                RegisterPostRegistrationTasks(assembly);
                RegisterCleanupTasks(assembly);
            }
		}

		private void RegisterHandlers(Assembly assembly)
		{
			var handlerInterfaceType = typeof(IHandler<,>);
			var handlers = (from t in assembly.GetTypes()
							from i in t.GetInterfaces()
							where i.IsGenericType &&
								  handlerInterfaceType.IsAssignableFrom(i.GetGenericTypeDefinition())
							select new { Interface = i, Implementation = t }).ToList();
			foreach (var handler in handlers)
				ServiceCollection.AddTransient(handler.Interface, handler.Implementation);
		}

		private void RegisterOneWayHandlers(Assembly assembly)
		{
			var handlerInterfaceType = typeof(IHandler<>);
			var handlers = (from t in assembly.GetTypes()
							from i in t.GetInterfaces()
							where i.IsGenericType &&
								  handlerInterfaceType.IsAssignableFrom(i.GetGenericTypeDefinition())
							select new { Interface = i, Implementation = t }).ToList();
			foreach (var handler in handlers)
				ServiceCollection.AddTransient(handler.Interface, handler.Implementation);
		}

		private void RegisterPostRegistrationTasks(Assembly assembly)
		{
			var registrationTaskType = typeof(IPostRegistrationTask);
			var registrationTaskTypes = (from t in assembly.GetTypes()
										 where registrationTaskType.IsAssignableFrom(t)
											   && t.IsClass
										 select t).ToList();
			registrationTaskTypes.ForEach(r => ServiceCollection.AddTransient(registrationTaskType, r));
		}

		private void RegisterCleanupTasks(Assembly assembly)
		{
			var cleanupTaskType = typeof(ICleanupTask);
			var cleanupTaskTypes = (from t in assembly.GetTypes()
									where cleanupTaskType.IsAssignableFrom(t)
										  && t.IsClass
									select t).ToList();
			cleanupTaskTypes.ForEach(r => ServiceCollection.AddTransient(cleanupTaskType, r));
		}
	}
}
