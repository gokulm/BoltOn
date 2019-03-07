using System.Reflection;
using BoltOn.Bootstrapping;

namespace BoltOn.Mediator.Data.EF
{
    public static class Extensions
    {
        public static BoltOnOptions BoltOnMediatorDataEF(this BoltOnOptions boltOnOptions)
		{
			boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			return boltOnOptions;
		}
    }
}
