using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace BoltOn.IoC.SimpleInjector
{
	
	/// <summary>
	/// Simple injectory container factory.
	/// In case if your application needs to modify the DefaultScopedLifestyle, ctor behavior or anything
	/// else, you could instantiate this factory by passing the container in the ctor
	/// </summary>
	public sealed class SimpleInjectoryContainerFactory : IBoltOnContainerFactory
	{
		private Container _container;

		public SimpleInjectoryContainerFactory()
		{
			_container = new Container();
		}

		public SimpleInjectoryContainerFactory(Container container)
		{
			_container = container;
		}

		public IBoltOnContainer Create()
		{
			_container.Options.ConstructorResolutionBehavior = new FewParameterizedConstructorBehavior();
			_container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
			return new SimpleInjectorContainerAdapter(_container);
		}
	}
}
