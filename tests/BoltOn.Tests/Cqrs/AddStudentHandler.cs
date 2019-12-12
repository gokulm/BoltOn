using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using BoltOn.Data;
using System;

namespace BoltOn.Tests.Cqrs
{
	public class AddStudentRequest : IRequest
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public bool RaiseAnotherCreateEvent { get; set; }
	}

	public class AddStudentHandler : IHandler<AddStudentRequest>
    {
        private readonly IBoltOnLogger<AddStudentHandler> _logger;
        private readonly IRepository<Student> _repository;

        public AddStudentHandler(IBoltOnLogger<AddStudentHandler> logger,
            IRepository<Student> repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task HandleAsync(AddStudentRequest request, CancellationToken cancellationToken)
        {
            _logger.Debug($"{nameof(AddStudentHandler)} invoked");
            var student = new Student(request.Name, request.Id);
            await _repository.AddAsync(student, cancellationToken);

			if(request.RaiseAnotherCreateEvent)
			{
				var student2 = new Student(request.Name + "2nd", CqrsConstants.Event3Id);
				await _repository.AddAsync(student2, cancellationToken);
			}
        }
    }
}
