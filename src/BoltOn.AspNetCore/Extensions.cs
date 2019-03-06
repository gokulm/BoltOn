using Microsoft.AspNetCore.Builder;

namespace BoltOn.AspNetCore
{
	public static class Extensions
	{
		public static void UseBoltOn(this IApplicationBuilder app)
		{
			app.ApplicationServices.UseBoltOn();
		}
	}
}
