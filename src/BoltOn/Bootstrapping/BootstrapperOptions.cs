using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoltOn.Context;
using BoltOn.Cqrs;
using BoltOn.Logging;
using BoltOn.Other;
using BoltOn.Requestor.Interceptors;
using BoltOn.Requestor.Pipeline;
using BoltOn.Transaction;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bootstrapping
{
	public sealed class BootstrapperOptions
	{
		private bool _isDisableRequestorHandlerRegistrations = false;

		internal bool IsCqrsEnabled { get; set; }
		internal bool IsTightened { get; set; }
		internal bool IsAppCleaned { get; set; }
		internal HashSet<Type> InterceptorTypes { get; set; } = new HashSet<Type>();
		internal Type RecentlyAddedInterceptor { get; set; }
		internal InterceptorOptions InterceptorOptions { get; }
		internal HashSet<Assembly> RegisteredAssemblies { get; } = new HashSet<Assembly>();
		public IServiceCollection ServiceCollection { get; }

		public BootstrapperOptions(IServiceCollection serviceCollection)
		{
			ServiceCollection = serviceCollection;
			InterceptorOptions = new InterceptorOptions(this);
			RegisterByConvention(GetType().Assembly);
			RegisterCoreTypes();
			RegisterRequestor();
		}

		public void BoltOnAssemblies(params Assembly[] assemblies)
		{
			var distinctAssemblies = assemblies.Distinct();
			RegisterByConvention(distinctAssemblies);
		}

		public void DisableRequestorHandlerRegistrations()
		{
			_isDisableRequestorHandlerRegistrations = true;
		}

		private void RegisterCoreTypes()
		{
			ServiceCollection.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
			ServiceCollection.AddSingleton<IAppLoggerFactory, AppLoggerFactory>();

			ServiceCollection.AddScoped<ScopedContext>();
			ServiceCollection.AddSingleton<Context.AppContext>();
		}

		private void RegisterRequestor()
		{
			ServiceCollection.AddTransient<IRequestor, Requestor.Pipeline.Requestor>();
			AddInterceptor<StopwatchInterceptor>();
			AddInterceptor<TransactionInterceptor>();
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
			var tempAssemblies = assemblies.ToList();
			var interfaces = (from assembly in tempAssemblies
							  from type in assembly.GetTypes()
							  where type.IsInterface
							  && !type.GetCustomAttributes(typeof(ExcludeFromRegistrationAttribute), true).Any()
							  select type).ToList();
			var tempRegistrations = (from @interface in interfaces
									 from assembly in tempAssemblies
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

			foreach (var assembly in tempAssemblies)
			{
				if (RegisteredAssemblies.Contains(assembly))
					continue;

				if (!_isDisableRequestorHandlerRegistrations)
				{
					RegisterHandlers(assembly);
					RegisterOneWayHandlers(assembly);
				}
				RegisterPostRegistrationTasks(assembly);
				RegisterCleanupTasks(assembly);

				RegisteredAssemblies.Add(assembly);
			}
		}

		private void RegisterHandlers(Assembly assembly)
		{
			var handlerInterfaceType = typeof(IHandler<,>);
			var handlers = (from t in assembly.GetTypes()
							from i in t.GetInterfaces()
							where i.IsGenericType &&
								!t.GetCustomAttributes(typeof(ExcludeFromRegistrationAttribute), true).Any() &&
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
								!t.GetCustomAttributes(typeof(ExcludeFromRegistrationAttribute), true).Any() &&
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
