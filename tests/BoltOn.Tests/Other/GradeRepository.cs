using System;
using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Data.CosmosDb;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Nito.AsyncEx;

namespace BoltOn.Tests.Other
{
    public interface IGradeRepository : IRepository<Grade>
    {
        void DeleteById<TId>(TId id, object partitionKey);
        Task DeleteByIdAsync<TId>(TId id, object partitionKey);
        Grade GetById<TId>(TId id, object partitionKey);
        Task<Grade> GetByIdAsync<TId>(TId id, object partitionKey);
    }

    public class GradeRepository : BaseCosmosDbRepository<Grade, CollegeDbContext>, IGradeRepository
    {
        public GradeRepository(CollegeDbContext collegeDbContext) : base(collegeDbContext)
        {
        }

        public virtual void DeleteById<TId>(TId id, object partitionKey)
        {
            AsyncContext.Run(() => DocumentClient.DeleteDocumentAsync(GetDocumentUri(id.ToString()), new RequestOptions { PartitionKey = new PartitionKey(partitionKey) }));
        }

        public virtual async Task DeleteByIdAsync<TId>(TId id, object partitionKey)
        {
            await DocumentClient.DeleteDocumentAsync(GetDocumentUri(id.ToString()), new RequestOptions { PartitionKey = new PartitionKey(partitionKey) });
        }

        public virtual Grade GetById<TId>(TId id, object partitionKey)
        {
            var document = AsyncContext.Run(() => DocumentClient.ReadDocumentAsync<Grade>(GetDocumentUri(id.ToString()), new RequestOptions { PartitionKey = new PartitionKey(partitionKey) }));
            return document.Document;
        }

        public virtual async Task<Grade> GetByIdAsync<TId>(TId id, object partitionKey)
        {
            return await DocumentClient.ReadDocumentAsync<Grade>(GetDocumentUri(id.ToString()), new RequestOptions { PartitionKey = new PartitionKey(partitionKey) });
        }
    }
}
