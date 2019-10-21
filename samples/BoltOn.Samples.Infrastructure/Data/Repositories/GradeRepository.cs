using System.Threading.Tasks;
using BoltOn.Data.CosmosDb;
using BoltOn.Samples.Application.Abstractions.Data;
using BoltOn.Samples.Application.Entities;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace BoltOn.Samples.Infrastructure.Data.Repositories
{
	public class GradeRepository : BaseCosmosDbRepository<Grade, CollegeDbContext>, IGradeRepository
    {
        public GradeRepository(CollegeDbContext collegeDbContext) : base(collegeDbContext)
        {
        }

        public virtual Grade GetById<TId>(TId id, object partitionKey)
        {
            var document = DocumentClient.ReadDocumentAsync<Grade>(GetDocumentUri(id.ToString()), 
				new RequestOptions { PartitionKey = new PartitionKey(partitionKey) }).GetAwaiter().GetResult();
            return document.Document;
        }

        public virtual async Task<Grade> GetByIdAsync<TId>(TId id, object partitionKey)
        {
            var document = await DocumentClient.ReadDocumentAsync<Grade>(GetDocumentUri(id.ToString()), 
				new RequestOptions { PartitionKey = new PartitionKey(partitionKey) });
            return document.Document;
        }
    }
}
