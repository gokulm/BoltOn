using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.Web.Authorization;
using BoltOn.Web.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Web
{
	public static class Extensions
	{
		private static bool _isWebModuleAdded;
		private static bool _isAuthorizationModuleAdded;

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

			if(_isAuthorizationModuleAdded)
			{
				app.UseAuthentication();
				app.UseAuthorization();
			}

			serviceProvider.TightenBolts();
		}

		public static BootstrapperOptions BoltOnAuthorizationModule(this BootstrapperOptions bootstrapperOptions)
		{
			_isAuthorizationModuleAdded = true;
			var serviceCollection = bootstrapperOptions.ServiceCollection;
			serviceCollection.AddScoped<IAuthorizationHandler, ScopeAuthorizationHandler>();
			serviceCollection.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
			serviceCollection.AddSingleton<IAuthorizationPolicyProvider, AppPolicyProvider>();
			return bootstrapperOptions;
		}
	}
}
