using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using BoltOn.Data;
using BoltOn.Cqrs;

namespace BoltOn.Tests.Cqrs
{
	public class StudentUpdatedEvent : CqrsEvent
	{
		public string Name { get; set; }

		public TestInput Input2 { get; set; }
	}

	public class TestCqrsUpdated2Event : CqrsEvent
	{
		public string Input1 { get; set; }

		public TestInput Input2 { get; set; }
	}

	public class StudentUpdatedEventHandler : IHandler<StudentUpdatedEvent>,
        IHandler<TestCqrsUpdated2Event>
    {
        private readonly IBoltOnLogger<StudentUpdatedEventHandler> _logger;
        private readonly IRepository<StudentFlattened> _repository;

        public StudentUpdatedEventHandler(IBoltOnLogger<StudentUpdatedEventHandler> logger,
            IRepository<StudentFlattened> repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task HandleAsync(StudentUpdatedEvent request, CancellationToken cancellationToken)
        {
            _logger.Debug($"{nameof(StudentUpdatedEventHandler)} invoked");
            var testCqrsReadEntity = await _repository.GetByIdAsync(request.SourceId);
            var isSuccessful = testCqrsReadEntity.UpdateInput(request);
            if (isSuccessful)
            {
                _logger.Debug($"{nameof(StudentFlattened)} updated. " +
                    $"Input1: {testCqrsReadEntity.FirstName} Input2Property1: {testCqrsReadEntity.Input2Property1} " +
                    $"Input2Propert2: {testCqrsReadEntity.Input2Property2}");
                // this is to avoid test warning, as TestCqrsUpdateEvent gets added in two different entities
                //await _repository.UpdateAsync(testCqrsReadEntity);
            }
        }

        public async Task HandleAsync(TestCqrsUpdated2Event request, CancellationToken cancellationToken)
        {
            _logger.Debug($"{nameof(StudentUpdatedEventHandler)} invoked for {nameof(TestCqrsUpdated2Event)}");
            await Task.CompletedTask;
        }
    }
}
