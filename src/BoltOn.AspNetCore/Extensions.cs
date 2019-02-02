using System;
using BoltOn.Utilities;
using Microsoft.AspNetCore.Builder;
using BoltOn.Bootstrapping;

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
