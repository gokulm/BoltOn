using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.Data.EF;

namespace BoltOn.Mediator.Data.EF
{
    public static class Extensions
    {
        public static BoltOnOptions BoltOnMediatorDataEF(this BoltOnOptions boltOnOptions)
		{
			boltOnOptions
				.BoltOnDataEF()
				.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			return boltOnOptions;
		}
    }
}
