using System.Collections.Generic;
using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace BoltOn.Samples.WebApi.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class PingController : ControllerBase
	{
		private readonly IMediator _mediator;

		public PingController(IMediator mediator)
		{
			this._mediator = mediator;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<string>>> Get()
		{
			var response = await _mediator.ProcessAsync(new PingRequest());
			return new string[] { response.Data };
		}
	}
}
