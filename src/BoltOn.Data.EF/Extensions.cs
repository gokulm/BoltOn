using System.Reflection;
using BoltOn.Bootstrapping;

namespace BoltOn.Data.EF
{
    public static class Extensions
    {
        public static BoltOnOptions AddEntityFrameworkModule(this BoltOnOptions boltOnOptions)
        {
			boltOnOptions.AddAssemblies(Assembly.GetExecutingAssembly());
			return boltOnOptions;
        }
    }
}
