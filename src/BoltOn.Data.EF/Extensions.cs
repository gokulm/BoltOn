using System.Reflection;
using BoltOn.Bootstrapping;

namespace BoltOn.Data.EF
{
    public static class Extensions
    {
        public static BoltOnOptions BoltOnDataEF(this BoltOnOptions boltOnOptions)
        {
			boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			return boltOnOptions;
        }
    }
}
