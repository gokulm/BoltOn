using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.IoC.NetStandardBolt
{
	public class NetStandardContainerAdapter : IBoltOnContainer
	{
		private IServiceCollection _serviceCollection;
		private IServiceProvider _serviceProvider;
		private bool _isDisposed;

		public NetStandardContainerAdapter()
		{
			_serviceCollection = new ServiceCollection();
		}

		public NetStandardContainerAdapter(IServiceCollection serviceCollection)
		{
			_serviceCollection = serviceCollection;
		}

		public void LockRegistration()
		{
			_serviceProvider = _serviceCollection.BuildServiceProvider();
		}

		public IEnumerable<TService> GetAllInstances<TService>() where TService : class
		{
			return _serviceProvider.GetServices<TService>();
		}

		public TService GetInstance<TService>() where TService : class
		{
			return _serviceProvider.GetService<TService>();
		}

		public object GetInstance(Type type)
		{
			return _serviceProvider.GetService(type);
		}

		public IBoltOnContainer RegisterScoped<TService, TImplementation>()
			where TService : class
			where TImplementation : class, TService
		{
			_serviceCollection.AddScoped<TService, TImplementation>();
			return this;
		}

		public IBoltOnContainer RegisterSingleton<TService, TImplementation>()
			where TService : class
			where TImplementation : class, TService
		{
			_serviceCollection.AddSingleton<TService, TImplementation>();
			return this;
		}

		public IBoltOnContainer RegisterSingleton(Type service, object implementationInstance)
		{
			_serviceCollection.AddSingleton(service, implementationInstance);
			return this;
		}

		public IBoltOnContainer RegisterTransient<TService, TImplementation>()
			where TService : class
			where TImplementation : class, TService
		{
			_serviceCollection.AddTransient<TService, TImplementation>();
			return this;
		}

		public IBoltOnContainer RegisterTransient<TService>() where TService : class
		{
			_serviceCollection.AddTransient<TService>();
			return this;
		}

		public IBoltOnContainer RegisterTransient(Type serviceType, Type implementation)
		{
			_serviceCollection.AddTransient(serviceType, implementation);
			return this;
		}

		public IBoltOnContainer RegisterTransient<TService>(Func<TService> implementationFactory)
			where TService : class
		{
			_serviceCollection.AddTransient(typeof(TService), s => implementationFactory());
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
					_serviceProvider = null;
					_serviceCollection = null;
				}
				_isDisposed = true;
			}
		}

		public IBoltOnContainer RegisterTransientCollection(Type serviceType, IEnumerable<Type> serviceTypes)
		{
			foreach (var tempServiceType in serviceTypes)
			{
				_serviceCollection.AddTransient(serviceType, tempServiceType);
			}
			return this;
		}

		public IBoltOnContainer RegisterTransientCollection<TService>(IEnumerable<Type> serviceTypes)
		{
			return RegisterTransientCollection(typeof(TService), serviceTypes);
		}

		public IBoltOnContainer RegisterScoped<TService>(Func<TService> implementationFactory) where TService : class
		{
			_serviceCollection.AddScoped(typeof(TService), s => implementationFactory());
			return this;
		}
	}

}
