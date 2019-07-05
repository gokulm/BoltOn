using System;
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
            var grade1 = new Grade { Id = Guid.NewGuid().ToString(), StudentId = 1, CourseName = "physics", Score = "A-", Year = 2017 };
            var grade2 = new Grade { Id = Guid.NewGuid().ToString(), StudentId = 2, CourseName = "computers", Score = "A-", Year = 2018 };

            var add = _gradeRepository.Add(grade1);
            var addasync = await _gradeRepository.AddAsync(grade2);

            var findby = _gradeRepository.FindBy(g => g.StudentId == 1);
            var findbyasync = await _gradeRepository.FindByAsync(g => g.StudentId == 2, cancellationToken);

            var getall = _gradeRepository.GetAll();
            var getallasync = await _gradeRepository.GetAllAsync();

            grade1.Year = 2019;
            grade2.Year = 2019;
            _gradeRepository.Update(grade1);
            await _gradeRepository.UpdateAsync(grade2);

            var getbyid = _gradeRepository.GetById(grade1.Id, grade1.StudentId);
            var getbyidasync = await _gradeRepository.GetByIdAsync(grade2.Id, grade2.StudentId);

            return new List<Grade>();
        }
    }
}
