using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.Data.EF;

namespace BoltOn.Mediator.Data.EF
{
    public static class Extensions
    {
        public static BoltOnOptions BoltOnMediatorEntityFramework(this BoltOnOptions boltOnOptions)
		{
			boltOnOptions
				.BoltOnEntityFramework()
				.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			return boltOnOptions;
		}
    }
}
