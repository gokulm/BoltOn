using System;

namespace BoltOn.IoC
{
	public class ServiceLocator 
	{
		static IServiceProvider _serviceProvider;

		public static IServiceProvider Current
		{
			get
			{
				if (_serviceProvider == null)
					throw new Exception("ServiceLocator not initialized");
				return _serviceProvider;
			}
		}

		internal static void SetContainer(IBoltOnContainer boltOnContainer)
		{
			_serviceProvider = boltOnContainer;
		}
	}
}
