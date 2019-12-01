using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using BoltOn.Data;
using BoltOn.Cqrs;

namespace BoltOn.Tests.Cqrs
{
	public class TestCqrsCreatedEvent : CqrsEvent
	{
		public string Input { get; set; }
	}

	public class TestCqrsCreatedEventHandler : IHandler<TestCqrsCreatedEvent>
    {
        private readonly IBoltOnLogger<TestCqrsCreatedEventHandler> _logger;
        private readonly IRepository<TestCqrsReadEntity> _repository;

        public TestCqrsCreatedEventHandler(IBoltOnLogger<TestCqrsCreatedEventHandler> logger,
            IRepository<TestCqrsReadEntity> repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task HandleAsync(TestCqrsCreatedEvent request, CancellationToken cancellationToken)
        {
            _logger.Debug($"{nameof(TestCqrsCreatedEventHandler)} invoked");
            var testCqrsReadEntity = new TestCqrsReadEntity(request);
			await _repository.AddAsync(testCqrsReadEntity, cancellationToken);
        }
    }
}
