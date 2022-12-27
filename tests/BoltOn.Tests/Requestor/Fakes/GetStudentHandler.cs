﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using BoltOn.Requestor;
using BoltOn.Tests.Other;

namespace BoltOn.Tests.Requestor.Fakes
{
	public class GetStudentRequest : IRequest<Student>
	{
		public int StudentId { get; set; }
	}

	public class GetStudentHandler : IHandler<GetStudentRequest, Student>
    {
        readonly IStudentRepository _studentRepository;

        public GetStudentHandler(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

		public async Task<Student> HandleAsync(GetStudentRequest request, CancellationToken cancellationToken)
		{
			var student = (await _studentRepository
                .FindByAsync(f => f.Id == request.StudentId, cancellationToken: cancellationToken)).FirstOrDefault();
			return student;
		}
	}
}
