using System.Collections.Generic;
using System.Threading.Tasks;
using BoltOn.Requestor.Pipeline;
using BoltOn.Samples.Application.DTOs;
using BoltOn.Samples.Application.Entities;
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
		public async Task<IEnumerable<StudentDto>> Get()
		{
			var students = await _requestor.ProcessAsync(new GetAllStudentsRequest());
			return students;
		}

		[HttpPost, Route("[controller]")]
		public async Task<Student> Post([FromBody]CreateStudentRequest request)
		{
			return await _requestor.ProcessAsync(request);
		}

		[HttpPut, Route("[controller]")]
		public async Task<string> Put([FromBody]UpdateStudentRequest request)
		{
			await _requestor.ProcessAsync(request);
			return "Updated";
		}
	}
}
