using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Abstractions.Data;
using BoltOn.Samples.Application.DTOs;

namespace BoltOn.Samples.Application.Handlers
{
	public class GetAllStudentsRequest : IQuery<IEnumerable<StudentDto>>
	{
	}

	public class GetAllStudentsHandler : IRequestAsyncHandler<GetAllStudentsRequest, IEnumerable<StudentDto>>
	{
		private readonly IStudentRepository _studentRepository;

		public GetAllStudentsHandler(IStudentRepository studentRepository)
		{
			_studentRepository = studentRepository;
		}

		public async Task<IEnumerable<StudentDto>> HandleAsync(GetAllStudentsRequest request, CancellationToken cancellationToken = default(CancellationToken))
		{
			var students = (await _studentRepository.GetAllAsync()).ToList();
			var studentDtos = from s in students
							   select new StudentDto
							   {
								   FirstName = s.FirstName,
								   LastName = s.LastName
							   };
			return studentDtos;
		}
	}
}
