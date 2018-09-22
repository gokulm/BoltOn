using System;

namespace BoltOn.IoC
{
	public class ServiceLocator 
	{
		static IServiceFactory _serviceFactory;

		public static IServiceFactory Current
		{
			get
			{
				if (_serviceFactory == null)
					throw new Exception("ServiceLocator not initialized");
				return _serviceFactory;
			}
		}

		internal static void SetContainer(IBoltOnContainer boltOnContainer)
		{
			_serviceFactory = boltOnContainer;
		}
	}
}
