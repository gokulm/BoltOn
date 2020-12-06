using System.Threading.Tasks;
using BoltOn.Requestor.Pipeline;
using BoltOn.Samples.Application.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace BoltOn.Samples.WebApi.Controllers
{
	[ApiController]
	public class PingController : ControllerBase
	{
		private readonly IRequestor _requestor;

		public PingController(IRequestor requestor)
		{
			_requestor = requestor;
		}

		[HttpGet, Route("[controller]")]
		public async Task<ActionResult<string>> Get()
		{
			var response = await _requestor.ProcessAsync(new PingRequest());
			return response;
		}
	}
}
