using BoltOn.Data;
using Microsoft.Azure.Documents.Client;

namespace BoltOn.Samples.Application.Abstractions.Data
{
    public interface IDocumentDbRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity<string>
    {
        FeedOptions FeedOptions { get; set; }
        RequestOptions RequestOptions { get; set; }
    }
}
