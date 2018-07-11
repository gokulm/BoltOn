namespace BoltOn.IoC.SimpleInjector
{
	public class SimpleInjectoryContainerFactory : IBoltOnContainerFactory
	{
		public IBoltOnContainer Create()
		{
			return new SimpleInjectorContainerAdapter();
		}
	}
}
