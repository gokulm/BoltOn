using System;
using System.Collections.Generic;

namespace BoltOn.IoC
{
	public interface IServiceLocator
	{
		IEnumerable<TService> GetAllInstances<TService>() where TService : class;
		TService GetInstance<TService>() where TService : class;
		object GetInstance(Type type);
	}

	public static class ServiceLocator
	{
		static IServiceLocator _serviceLocator;

		public static IServiceLocator Current
		{
			get
			{
				if (_serviceLocator == null)
					throw new Exception("ServiceLocator not initialized");
				return _serviceLocator;
			}
		}

		internal static void SetContainer(IBoltOnContainer boltOnContainer)
		{
			_serviceLocator = boltOnContainer;
		}
	}
}
