using System.Collections.Generic;
using System.Security.Claims;

namespace BoltOn.Web.Authorization
{
	public interface IClaimsService
	{
		void LoadClaims(ClaimsPrincipal claimsPrincipal);
	}

	public class DefaultClaimsService : IClaimsService
	{
		public void LoadClaims(ClaimsPrincipal claimsPrincipal)
		{
		}
	}
}

