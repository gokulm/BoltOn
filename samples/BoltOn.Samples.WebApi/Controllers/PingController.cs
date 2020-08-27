using System.Threading.Tasks;
using BoltOn.Requestor.Pipeline;
using BoltOn.Samples.Application.Handlers;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace BoltOn.Samples.WebApi.Controllers
{
	[ApiController]
	public class PingController : ControllerBase
	{
		private readonly IRequestor _requestor;
		private readonly IBackgroundJobClient _backgroundJobClient;

		public PingController(IRequestor requestor,
			IBackgroundJobClient backgroundJobClient)
		{
			_requestor = requestor;
			_backgroundJobClient = backgroundJobClient;
		}

		[HttpGet, Route("[controller]")]
		public async Task<ActionResult<string>> Get()
		{
			_backgroundJobClient.Enqueue<IRequestor>(r => r.ProcessAsync(new PingRequest(), System.Threading.CancellationToken.None));

			//var response = await _requestor.ProcessAsync(new PingRequest());
			return await Task.FromResult("test");
		}
	}
}
