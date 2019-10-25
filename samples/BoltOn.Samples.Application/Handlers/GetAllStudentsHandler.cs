using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.DTOs;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
	public class GetAllStudentsRequest : IQuery<IEnumerable<StudentDto>>
    {
    }

    public class GetAllStudentsHandler : IRequestAsyncHandler<GetAllStudentsRequest, IEnumerable<StudentDto>>
    {
        private readonly IRepository<StudentFlattened> _studentRepository;

        public GetAllStudentsHandler(IRepository<StudentFlattened> studentRepository)
        {
            _studentRepository = studentRepository;
        }
        
		public async Task<IEnumerable<StudentDto>> HandleAsync(GetAllStudentsRequest request, 
			CancellationToken cancellationToken = default)
		{
			var students = (await _studentRepository.GetAllAsync()).ToList();
			var studentDtos = from s in students
							   select new StudentDto
							   {
								   Id = s.Id,
								   FirstName = s.FirstName,
								   LastName = s.LastName,
								   StudentType = s.StudentType
							   };
			return studentDtos;
		}
	}
}
