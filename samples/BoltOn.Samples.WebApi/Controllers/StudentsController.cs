using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.DTOs;
using BoltOn.Samples.Application.Entities;
using BoltOn.Samples.Application.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace BoltOn.Samples.WebApi.Controllers
{
	public class StudentsController : Controller
	{
		private readonly IMediator _mediator;

		public StudentsController(IMediator mediator)
		{
			this._mediator = mediator;
		}

		[HttpGet, Route("[controller]")]
		public async Task<IEnumerable<StudentDto>> Get()
		{
			var students = await _mediator.ProcessAsync(new GetAllStudentsRequest());
			return students;
		}

		[HttpPost, Route("[controller]")]
		public async Task<Guid> Post(CreateStudentRequest request)
		{
			return await _mediator.ProcessAsync(request);
		}

		[HttpPut, Route("[controller]")]
		public async Task<string> Put(UpdateStudentRequest request)
		{
			await _mediator.ProcessAsync(request);
			return "Updated";
		}

		[Route("{studentId}/grades")]
		[HttpGet]
		public async Task<IEnumerable<Grade>> GetGrades([FromRoute] GetAllGradesRequest request)
		{
			return await _mediator.ProcessAsync(request);
		}
	}
}
