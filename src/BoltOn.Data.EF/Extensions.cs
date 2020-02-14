using System.Reflection;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Data.EF
{
    public static class Extensions
    {
        public static BoltOnOptions BoltOnEFModule(this BoltOnOptions boltOnOptions)
        {
            boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
            boltOnOptions.ServiceCollection.AddScoped<ChangeTrackerContext>();
            boltOnOptions.AddInterceptor<ChangeTrackerInterceptor>();
            return boltOnOptions;
        }
    }
}
