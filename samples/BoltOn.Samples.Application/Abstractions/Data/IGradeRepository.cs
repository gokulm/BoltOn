using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Samples.Domain.Entities;

namespace BoltOn.Samples.Application.Abstractions.Data
{
    public interface IGradeRepository : IRepository<Grade>
    {
        Grade GetById<TId>(TId id, object partitionKey);
        Task<Grade> GetByIdAsync<TId>(TId id, object partitionKey);
    }
}
