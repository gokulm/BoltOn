using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.Web.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace BoltOn.Web
{
	public static class Extensions
    {
        private static bool _isWebModuleAdded;

        public static BootstrapperOptions BoltOnWebModule(this BootstrapperOptions bootstrapperOptions)
        {
            _isWebModuleAdded = true;
            bootstrapperOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
            return bootstrapperOptions;
        }

        public static void TightenBolts(this IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices;
            if (_isWebModuleAdded)
            {
                app.UseMiddleware<RequestLoggerContextMiddleware>();
            }
            serviceProvider.TightenBolts();
        }
    }
}
