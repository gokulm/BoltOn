using System;
using System.Collections.Generic;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace BoltOn.IoC.SimpleInjector
{
	public class SimpleInjectorContainerAdapter : IBoltOnContainer
	{
		private Container _container;
		private bool _isDisposed;

		public SimpleInjectorContainerAdapter()
		{
			_container = new Container();
			_container.Options.ConstructorResolutionBehavior = new FewParameterizedConstructorBehavior();
			_container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
		}

		public SimpleInjectorContainerAdapter(Container container)
		{
			_container = container; 
		}

		public IEnumerable<TService> GetAllInstances<TService>() where TService : class
		{
			return _container.GetAllInstances<TService>();
		}

		public TService GetInstance<TService>() where TService : class
		{
			return _container.GetInstance<TService>();
		}

		public void LockRegistration()
		{
		}

		public IBoltOnContainer RegisterTransient<TService>() where TService : class
		{
			_container.Register<TService>();
			return this;
		}

		public IBoltOnContainer RegisterTransient(Type serviceType, Type implementation)
		{
			_container.Register(serviceType, implementation);
			return this;
		}

		public IBoltOnContainer RegisterScoped<TService, TImplementation>()
			where TService : class
			where TImplementation : class, TService
		{
			_container.Register<TService, TImplementation>(Lifestyle.Scoped);
			return this;
		}

		public IBoltOnContainer RegisterSingleton<TService, TImplementation>()
			where TService : class
			where TImplementation : class, TService
		{
			_container.RegisterSingleton<TService, TImplementation>();
			return this;
		}

		public IBoltOnContainer RegisterTransient<TService, TImplementation>()
			where TService : class
			where TImplementation : class, TService
		{
			_container.Register<TService, TImplementation>();
			return this;
		}

		public IBoltOnContainer RegisterTransient<TService>(Func<TService> implementationFactory) where TService : class
		{
			_container.Register(typeof(TService), implementationFactory);
			return this;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					if (_container != null)
					{
						_container.Dispose();
						_container = null;
					}
				}
				_isDisposed = true;
			}
		}

		public IBoltOnContainer RegisterTransientCollection(Type serviceType, IEnumerable<Type> serviceTypes)
		{
			_container.Collection.Register(serviceType, serviceTypes);
			return this;
		}
	}
}
