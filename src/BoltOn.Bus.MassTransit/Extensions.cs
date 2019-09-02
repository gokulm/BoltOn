using System.Reflection;
using BoltOn.Bootstrapping;

namespace BoltOn.Bus.MassTransit
{
	public static class Extensions
	{
		public static BoltOnOptions BoltOnMassTransitBusModule(this BoltOnOptions boltOnOptions)
		{
			boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			return boltOnOptions;
		}
	}
}
