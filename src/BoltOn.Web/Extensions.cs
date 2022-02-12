using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.Web.Authorization;
using BoltOn.Web.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

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
				app.UseMiddleware<UserClaimsMiddleware>();
				app.UseAuthorization();
			}

			serviceProvider.TightenBolts();
		}

		public static BootstrapperOptions BoltOnAuthorizationModule(this BootstrapperOptions bootstrapperOptions)
		{
			_isAuthorizationModuleAdded = true;
			var serviceCollection = bootstrapperOptions.ServiceCollection;
			serviceCollection.AddTransient<IClaimsService, DefaultClaimsService>();
			serviceCollection.AddScoped<IAuthorizationHandler, ScopeAuthorizationHandler>();
			serviceCollection.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
			serviceCollection.AddSingleton<IAuthorizationPolicyProvider, AppPolicyProvider>();
			return bootstrapperOptions;
		}
	}
}
