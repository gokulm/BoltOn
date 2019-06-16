using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Abstractions.Data;
using BoltOn.Samples.Application.DTOs;
using BoltOn.Samples.Domain.Entities;
using Microsoft.Azure.Documents.Client;

namespace BoltOn.Samples.Application.Handlers
{
    public class GetAllStudentsRequest : IQuery<IEnumerable<StudentDto>>
    {
    }

    public class GetAllStudentsHandler : IRequestAsyncHandler<GetAllStudentsRequest, IEnumerable<StudentDto>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IGradeRepository _gradeRepository;

        public GetAllStudentsHandler(IStudentRepository studentRepository, IGradeRepository gradeRepository)
        {
            _studentRepository = studentRepository;
            _gradeRepository = gradeRepository;
        }

        public async Task<IEnumerable<StudentDto>> HandleAsync(GetAllStudentsRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            //_gradeRepository.Init(new RequestOptions()); //To set either requestoptions/feedoptions
            var result = _gradeRepository.GetAll();

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
