using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Requestor.Pipeline;
using BoltOn.Samples.Application.DTOs;
using BoltOn.Samples.Application.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace BoltOn.Samples.WebApi.Controllers
{
	public class StudentsController : Controller
	{
		private readonly IRequestor _requestor;

		public StudentsController(IRequestor requestor)
		{
			_requestor = requestor;
		}

		[HttpGet, Route("[controller]")]
		public async Task<IEnumerable<StudentDto>> Get(CancellationToken cancellationToken)
		{
			var students = await _requestor.ProcessAsync(new GetAllStudentsRequest(), cancellationToken);
			return students;
		}

		[HttpPost, Route("[controller]")]
		public async Task<StudentDto> Post([FromBody] CreateStudentRequest request,
			CancellationToken cancellationToken)
		{
			return await _requestor.ProcessAsync(request, cancellationToken);
		}

		[HttpPut, Route("[controller]")]
		public async Task<string> Put([FromBody] UpdateStudentRequest request,
			CancellationToken cancellationToken)
		{
			await _requestor.ProcessAsync(request, cancellationToken);
			return "Updated";
		}

		[HttpGet, Route("[controller]/{studentId}")]
		public async Task<StudentDto> GetStudent([FromRoute] GetStudentRequest request,
			CancellationToken cancellationToken)
		{
			var result = await _requestor.ProcessAsync(request, cancellationToken);
			return result;
		}

		[HttpPut, Route("[controller]/{studentId}/courses/{courseId}")]
		public async Task<IActionResult> EnrollCourse([FromRoute] Guid studentId, [FromRoute] Guid courseId,
			CancellationToken cancellationToken)
		{
			var request = new EnrollCourseRequest { StudentId = studentId, CourseId = courseId };
			await _requestor.ProcessAsync(request, cancellationToken);
			return Ok("Enrolled");
		}
	}
}
