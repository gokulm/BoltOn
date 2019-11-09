using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using BoltOn.Data;

namespace BoltOn.Tests.Cqrs
{
	public class TestCqrsRequest : IRequest
	{
		public string Input { get; set; }
	}

	public class TestCqrsHandler : IRequestAsyncHandler<TestCqrsRequest>,
        IRequestHandler<TestCqrsRequest>
    {
        private readonly IBoltOnLogger<TestCqrsHandler> _logger;
        private readonly IRepository<TestCqrsWriteEntity> _repository;

        public TestCqrsHandler(IBoltOnLogger<TestCqrsHandler> logger,
            IRepository<TestCqrsWriteEntity> repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public void Handle(TestCqrsRequest request)
        {
            _logger.Debug($"{nameof(TestCqrsHandler)} invoked");
            var testCqrsWriteEntity = _repository.GetByIdAsync(CqrsConstants.EntityId).GetAwaiter().GetResult();
            testCqrsWriteEntity.ChangeInput(request);
            _repository.UpdateAsync(testCqrsWriteEntity).GetAwaiter().GetResult();
        }

        public async Task HandleAsync(TestCqrsRequest request, CancellationToken cancellationToken)
        {
            _logger.Debug($"{nameof(TestCqrsHandler)} invoked");
            var testCqrsWriteEntity = await _repository.GetByIdAsync(CqrsConstants.EntityId);
            testCqrsWriteEntity.ChangeInput(request);
            await _repository.UpdateAsync(testCqrsWriteEntity, cancellationToken);
        }
    }
}
