using BoltOn.Data;
using BoltOn.Data.EF;
using BoltOn.Samples.Application.Abstractions.Data;

namespace BoltOn.Samples.Infrastructure.Data.Repositories
{
    public class DocumentDbRepository<TEntity> : BaseDocumentDbRepository<TEntity>, IDocumentDbRepository<TEntity> where TEntity : BaseEntity<string>
    {
        public DocumentDbRepository() : base("dbname", "collectionname")
        {
        }
    }
}
