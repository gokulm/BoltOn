using System.Collections.Generic;
using System.Threading.Tasks;
using BoltOn.Bus;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.DTOs;
using BoltOn.Samples.Application.Handlers;
using BoltOn.Samples.Application.Messages;
using BoltOn.Samples.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BoltOn.Samples.WebApi.Controllers
{
	[Route("api/[controller]")]
	public class StudentsController : Controller
	{
		private readonly IMediator _mediator;
		private readonly IBus _bus;

		public StudentsController(IMediator mediator, IBus bus)
		{
			this._mediator = mediator;
			this._bus = bus;
		}

		[HttpGet]
		public async Task<IEnumerable<StudentDto>> Get()
		{
			var students = await _mediator.ProcessAsync(new GetAllStudentsRequest());
			return students;
		}
		
        [Route("{studentId}/Grades")]
        [HttpGet]
        public async Task<IEnumerable<Grade>> GetGrades([FromRoute] GetAllGradesRequest request)
        {
            return await _mediator.ProcessAsync(request);
        }

		[HttpPost]
		public async Task<StudentDto> Post()
		{
			await _bus.PublishAsync(new CreateStudent { FirstName = "new" });
			return new StudentDto { FirstName = "test", LastName = "test" };
		}
	}
}
