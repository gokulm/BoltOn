using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.IoC.NetStandardBolt
{
	/// <summary>
	/// Net standard container factory.
	/// In case if your application needs to modify the default behavior of ServiceCollection, you
	/// you could instantiate this factory and pass the container in the ctor
	/// </summary>
	public sealed class NetStandardContainerFactory : IBoltOnContainerFactory
    {
		private ServiceCollection _serviceCollection;

		public NetStandardContainerFactory()
		{
			_serviceCollection = new ServiceCollection();
		}

		public NetStandardContainerFactory(ServiceCollection serviceCollection)
		{
			_serviceCollection = serviceCollection ?? new ServiceCollection();
		}

		public IBoltOnContainer Create()
        {
			return new NetStandardContainerAdapter(_serviceCollection);
        }
	}
}
