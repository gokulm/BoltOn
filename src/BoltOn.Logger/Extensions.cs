using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BoltOn.Logger
{
    public static class Extensions
    {
        public static BootstrapperOptions BoltOnLoggerModule(this BootstrapperOptions bootstrapperOptions)
        {
            bootstrapperOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
            bootstrapperOptions.ServiceCollection.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
            bootstrapperOptions.ServiceCollection.AddSingleton<IAppLoggerFactory, AppLoggerFactory>();
            return bootstrapperOptions;
        }
    }
}
