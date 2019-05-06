using BoltOn.Data;
using BoltOn.Data.EF;
using BoltOn.Samples.Application.Abstractions.Data;
using Microsoft.Azure.Documents.Client;

namespace BoltOn.Samples.Infrastructure.Data.Repositories
{
    public class DocumentDbRepository<TEntity> : BaseDocumentDbRepository<TEntity>, IDocumentDbRepository<TEntity> where TEntity : BaseEntity<string>
    {
        public DocumentDbRepository() : base("dbname", "collectionname")
        {
        }

        public FeedOptions FeedOptions { get; set; }
        public RequestOptions RequestOptions { get; set; }

        protected override RequestOptions requestOptions
        {
            get
            {
                return RequestOptions;
            }
            set
            {
                RequestOptions = value;
            }
        }

        protected override FeedOptions feedOptions
        {
            get
            {
                return FeedOptions;
            }
            set
            {
                FeedOptions = value;
            }
        }
    }
}
