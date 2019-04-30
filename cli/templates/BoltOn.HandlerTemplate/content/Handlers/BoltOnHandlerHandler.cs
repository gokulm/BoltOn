using System;
using BoltOn;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Handler
{
    public class BoltOnHandlerRequest : IQuery<IEnumerable<StudentDto>>
	{
	}

	public class BoltOnHandlerHandler : IRequestAsyncHandler<GetAllStudentsRequest, IEnumerable<StudentDto>>
	{
		private readonly IStudentRepository _studentRepository;

		public BoltOnHandlerHandler(IStudentRepository studentRepository)
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
