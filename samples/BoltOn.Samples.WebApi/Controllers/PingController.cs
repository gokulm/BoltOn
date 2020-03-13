using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace BoltOn.Samples.WebApi.Controllers
{
	[ApiController]
	public class PingController : ControllerBase
	{
		private readonly IMediator _mediator;

		public PingController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet, Route("[controller]")]
		public async Task<ActionResult<string>> Get()
		{
			var response = await _mediator.ProcessAsync(new PingRequest());
			return response.Data;
		}
	}
}
