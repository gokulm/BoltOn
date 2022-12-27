using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Data.EF;
using BoltOn.Logging;
using BoltOn.Requestor;

namespace BoltOn.Tests.Cqrs.Fakes
{
	public class AddStudentRequest : IRequest
    {
        public Guid StudentId { get; set; }
        public Guid EventId { get; set; }
        public string Name { get; set; }
        public bool RaiseAnotherCreateEvent { get; set; }
        public bool PurgeEvents { get; set; } = true;
	}

    public class AddStudentHandler : IHandler<AddStudentRequest>
    {
        private readonly IAppLogger<AddStudentHandler> _logger;
        private readonly IRepository<Student> _repository;

        public AddStudentHandler(IAppLogger<AddStudentHandler> logger,
            IRepository<Student> repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task HandleAsync(AddStudentRequest request, CancellationToken cancellationToken)
        {
            _logger.Debug($"{nameof(AddStudentHandler)} invoked");
            var student = new Student(request.Name, request.StudentId, purgeEvents: request.PurgeEvents);
            await _repository.AddAsync(student, cancellationToken);

            if (request.RaiseAnotherCreateEvent)
            {
                var student2 = new Student(request.Name + "2nd", CqrsConstants.Student1Id, request.EventId);
                await _repository.AddAsync(student2, cancellationToken);
            }
        }
    }
}
