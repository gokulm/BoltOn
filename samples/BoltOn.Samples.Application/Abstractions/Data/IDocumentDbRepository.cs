using BoltOn.Data;

namespace BoltOn.Samples.Application.Abstractions.Data
{
    public interface IDocumentDbRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity<string>
    {
    }
}
