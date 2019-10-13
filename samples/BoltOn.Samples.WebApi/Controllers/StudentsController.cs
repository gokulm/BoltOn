using System.Collections.Generic;
using System.Threading.Tasks;
using BoltOn.Bus;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.DTOs;
using BoltOn.Samples.Application.Entities;
using BoltOn.Samples.Application.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace BoltOn.Samples.WebApi.Controllers
{
	[Route("api/[controller]")]
	public class StudentsController : Controller
	{
		private readonly IMediator _mediator;

		public StudentsController(IMediator mediator)
		{
			this._mediator = mediator;
		}

		[HttpGet]
		public async Task<IEnumerable<StudentDto>> Get()
		{
			var students = await _mediator.ProcessAsync(new GetAllStudentsRequest());
			return students;
		}
		
        [Route("{studentId}/grades")]
        [HttpGet]
        public async Task<IEnumerable<Grade>> GetGrades([FromRoute] GetAllGradesRequest request)
        {
            return await _mediator.ProcessAsync(request);
        }

		[HttpPost]
		public async Task<StudentDto> Post(CreateStudentRequest request)
		{
			await _mediator.ProcessAsync(request);
			return new StudentDto { FirstName = request.FirstName, LastName = request.FirstName };
		}
	}
}
