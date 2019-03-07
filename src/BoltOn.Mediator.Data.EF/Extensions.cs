using System.Reflection;
using BoltOn.Bootstrapping;

namespace BoltOn.Mediator.Data.EF
{
    public static class Extensions
    {
        public static void BoltOnMediatorDataEF(this BoltOnOptions boltOnOptions)
		{
			boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
		}
    }
}
