using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using BoltOn.Data;

namespace BoltOn.Tests.Cqrs
{
	public class CreateTestCqrsRequest : IRequest
	{
		public string Input { get; set; }
	}

	public class CreateTestCqrsHandler : IHandler<CreateTestCqrsRequest>
    {
        private readonly IBoltOnLogger<CreateTestCqrsHandler> _logger;
        private readonly IRepository<TestCqrsWriteEntity> _repository;

        public CreateTestCqrsHandler(IBoltOnLogger<CreateTestCqrsHandler> logger,
            IRepository<TestCqrsWriteEntity> repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task HandleAsync(CreateTestCqrsRequest request, CancellationToken cancellationToken)
        {
            _logger.Debug($"{nameof(CreateTestCqrsHandler)} invoked");
            var testCqrsWriteEntity = new TestCqrsWriteEntity(request.Input);
            await _repository.AddAsync(testCqrsWriteEntity, cancellationToken);
        }
    }
}
