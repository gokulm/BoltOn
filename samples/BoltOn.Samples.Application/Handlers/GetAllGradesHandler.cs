using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Abstractions.Data;
using BoltOn.Samples.Domain.Entities;

namespace BoltOn.Samples.Application.Handlers
{
    public class GetAllGradesRequest : IQuery<IEnumerable<Grade>>
    {
        public int StudentId { get; set; }
    }

    public class GetAllGradesHandler : IRequestAsyncHandler<GetAllGradesRequest, IEnumerable<Grade>>
    {
        private readonly IGradeRepository _gradeRepository;

        public GetAllGradesHandler(IGradeRepository gradeRepository)
        {
            _gradeRepository = gradeRepository;
        }

        public async Task<IEnumerable<Grade>> HandleAsync(GetAllGradesRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _gradeRepository.FindByAsync(g => g.StudentId == request.StudentId, cancellationToken);
        }
    }
}
