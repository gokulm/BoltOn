using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Abstractions.Data;
using BoltOn.Samples.Domain.Entities;
using Microsoft.Azure.Documents.Client;

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
            _gradeRepository.Init(new FeedOptions { EnableCrossPartitionQuery = true }); //Only if you have to set either requestoptions/feedoptions

            return await _gradeRepository.FindByAsync(g => g.StudentId == request.StudentId, cancellationToken);
        }
    }
}
