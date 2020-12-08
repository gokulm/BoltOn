using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.Web.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace BoltOn.Web
{
	public static class Extensions
    {
        private static bool _isWebModuleAdded;

        public static BoltOnOptions AddWebModule(this BoltOnOptions boltOnOptions)
        {
            _isWebModuleAdded = true;
            boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
            return boltOnOptions;
        }

        public static void TightenBolts(this IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices;
            if (_isWebModuleAdded)
            {
                app.UseMiddleware<RequestLoggingMiddleware>();
            }
            serviceProvider.TightenBolts();
        }
    }
}
