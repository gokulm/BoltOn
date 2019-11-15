using System.Collections.Generic;
using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace BoltOn.Samples.WebApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ValuesController : ControllerBase
	{
		private readonly IMediator _mediator;

		public ValuesController(IMediator mediator)
		{
			this._mediator = mediator;
		}

		// GET api/values
		[HttpGet]
		public async Task<ActionResult<IEnumerable<string>>> Get()
		{
			var response = await _mediator.ProcessAsync(new PingRequest());
			return new string[] { response.Data };
		}

		// GET api/values/5
		[HttpGet("{id}")]
		public ActionResult<string> Get(int id)
		{
			return "value";
		}

		// POST api/values
		[HttpPost]
		public void Post([FromBody] string value)
		{
		}

		// PUT api/values/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/values/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}
