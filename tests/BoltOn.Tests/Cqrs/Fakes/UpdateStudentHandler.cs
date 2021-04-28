﻿using System.Threading;
using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Logging;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Tests.Cqrs.Fakes
{
    public class UpdateStudentRequest : IRequest
    {
        public string Input { get; set; }
    }

    public class UpdateStudentHandler : IHandler<UpdateStudentRequest>
    {
        private readonly IAppLogger<UpdateStudentHandler> _logger;
        private readonly IRepository<Student> _repository;

        public UpdateStudentHandler(IAppLogger<UpdateStudentHandler> logger,
            IRepository<Student> repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task HandleAsync(UpdateStudentRequest request, CancellationToken cancellationToken)
        {
            _logger.Debug($"{nameof(UpdateStudentHandler)} invoked");
            var student = await _repository.GetByIdAsync(CqrsConstants.EntityId);
            student.Modify(request);
            await _repository.UpdateAsync(student, cancellationToken);
        }
    }
}
