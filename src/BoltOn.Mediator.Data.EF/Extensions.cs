using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.Data.EF;

namespace BoltOn.Mediator.Data.EF
{
    public static class Extensions
    {
        public static BoltOnOptions BoltOnMediatorEFModule(this BoltOnOptions boltOnOptions)
		{
			boltOnOptions
				.BoltOnEFModule()
				.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			return boltOnOptions;
		}
    }
}
