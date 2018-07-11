using System;

namespace BoltOn.IoC
{
    public interface IBoltOnContainer : IServiceLocator, IDisposable
    {
        IBoltOnContainer RegisterScoped<TService, TImplementation>() where TService : class
            where TImplementation : class, TService;
        IBoltOnContainer RegisterTransient<TService, TImplementation>() where TService : class
            where TImplementation : class, TService;
        IBoltOnContainer RegisterSingleton<TService, TImplementation>() where TService : class
            where TImplementation : class, TService;
        IBoltOnContainer RegisterTransient<TService>() where TService : class;
        IBoltOnContainer RegisterTransient(Type serviceType, Type implementation);
        IBoltOnContainer RegisterTransient<TService>(Func<TService> implementationFactory) where TService : class;
        void LockRegistration();
    }

	public interface IBoltOnContainerFactory
	{
		IBoltOnContainer Create();
	}
}
