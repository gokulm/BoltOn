using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.Data.EF;

namespace BoltOn.Mediator.Data.EF
{
    public static class Extensions
    {
        public static BoltOnOptions AddMediatorEntityFrameworkModule(this BoltOnOptions boltOnOptions)
		{
			boltOnOptions
				.AddEntityFrameworkModule()
				.AddAssemblies(Assembly.GetExecutingAssembly());
			return boltOnOptions;
		}
    }
}
