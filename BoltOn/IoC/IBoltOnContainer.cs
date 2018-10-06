using System;
using System.Collections.Generic;

namespace BoltOn.IoC
{
	public interface IBoltOnContainer : IServiceFactory, IDisposable
	{
		IBoltOnContainer RegisterScoped<TService, TImplementation>() where TService : class
			where TImplementation : class, TService;
		IBoltOnContainer RegisterTransient<TService, TImplementation>() where TService : class
			where TImplementation : class, TService;
		IBoltOnContainer RegisterSingleton<TService, TImplementation>() where TService : class
			where TImplementation : class, TService;
		IBoltOnContainer RegisterSingleton(Type service, object implementationInstance);
		IBoltOnContainer RegisterTransient<TService>() where TService : class;
		IBoltOnContainer RegisterTransient(Type serviceType, Type implementation);
		IBoltOnContainer RegisterTransient<TService>(Func<TService> implementationFactory) where TService : class;
		IBoltOnContainer RegisterTransientCollection(Type serviceType, IEnumerable<Type> implementationTypes);
		IBoltOnContainer RegisterTransientCollection<TService>(IEnumerable<Type> implementationTypes);
		void LockRegistration();
	}
}
