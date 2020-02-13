using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;
using BoltOn.Other;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bootstrapping
{
	public sealed class BoltOnOptions
	{
        internal bool IsCqrsEnabled { get; set; }

		internal List<object> OtherOptions { get; } = new List<object>();

		public IServiceCollection ServiceCollection { get; }

		public BoltOnOptions(IServiceCollection serviceCollection)
		{
			ServiceCollection = serviceCollection;
            RegisterByConvention(GetType().Assembly);
		}

		public void BoltOnAssemblies(params Assembly[] assemblies)
		{
			//AssembliesToBeIncluded.AddRange(assemblies);
            RegisterByConvention(assemblies);
		}

        public void AddInterceptor<TInterceptor>() where TInterceptor : IInterceptor
        {
            ServiceCollection.AddTransient(typeof(IInterceptor), typeof(TInterceptor));
        }

        public void RemoveInterceptor<TInterceptor>() where TInterceptor : IInterceptor
        {
            var serviceDescriptor = ServiceCollection.FirstOrDefault(descriptor =>
                descriptor.ServiceType == typeof(TInterceptor));
            if (serviceDescriptor != null)
                ServiceCollection.Remove(serviceDescriptor);
        }

        public void RemoveAllInterceptors()
        {
            var serviceDescriptor = ServiceCollection.FirstOrDefault(descriptor =>
                descriptor.ServiceType == typeof(IInterceptor));
            if (serviceDescriptor != null)
                ServiceCollection.Remove(serviceDescriptor);
        }

        internal void RegisterByConvention(Assembly assembly)
        {
            RegisterByConvention( new List<Assembly> { assembly });
        }

        private void RegisterByConvention(IEnumerable<Assembly> assemblies)
        {
            var tempAssemblies = assemblies.ToList();
            var interfaces = (from assembly in tempAssemblies
                              from type in assembly.GetTypes()
                              where type.IsInterface
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

            RegisterHandlers(tempAssemblies);
            RegisterOneWayHandlers(tempAssemblies);
            RegisterPostRegistrationTasks(tempAssemblies);
        }

        private void RegisterHandlers(IEnumerable<Assembly> assemblies)
        {
            var handlerInterfaceType = typeof(IHandler<,>);
            var handlers = (from a in assemblies
                            from t in a.GetTypes()
                            from i in t.GetInterfaces()
                            where i.IsGenericType &&
                                  handlerInterfaceType.IsAssignableFrom(i.GetGenericTypeDefinition())
                            select new { Interface = i, Implementation = t }).ToList();
            foreach (var handler in handlers)
                ServiceCollection.AddTransient(handler.Interface, handler.Implementation);
        }

        private void RegisterOneWayHandlers(IEnumerable<Assembly> assemblies)
        {
            var handlerInterfaceType = typeof(IHandler<>);
            var handlers = (from a in assemblies
                            from t in a.GetTypes()
                            from i in t.GetInterfaces()
                            where i.IsGenericType &&
                                  handlerInterfaceType.IsAssignableFrom(i.GetGenericTypeDefinition())
                            select new { Interface = i, Implementation = t }).ToList();
            foreach (var handler in handlers)
                ServiceCollection.AddTransient(handler.Interface, handler.Implementation);
        }

        private void RegisterPostRegistrationTasks(IEnumerable<Assembly> assemblies)
        {
            var registrationTaskType = typeof(IPostRegistrationTask);
            var registrationTaskTypes = (from a in assemblies
                                         from t in a.GetTypes()
                                         where registrationTaskType.IsAssignableFrom(t)
                                               && t.IsClass
                                         select t).ToList();
            registrationTaskTypes.ForEach(r => ServiceCollection.AddTransient(registrationTaskType, r));
        }

        private void RegisterCleanupTasks(IEnumerable<Assembly> assemblies)
        {
            var cleanupTaskType = typeof(ICleanupTask);
            var cleanupTaskTypes = (from a in assemblies
                from t in a.GetTypes()
                where cleanupTaskType.IsAssignableFrom(t)
                      && t.IsClass
                select t).ToList();
            cleanupTaskTypes.ForEach(r => ServiceCollection.AddTransient(cleanupTaskType, r));
        }
    }
}
