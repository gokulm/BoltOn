using System.Threading;
using System.Threading.Tasks;
using BoltOn.Data.CosmosDb;
using BoltOn.Samples.Application.Abstractions.Data;
using BoltOn.Samples.Application.Entities;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace BoltOn.Samples.Infrastructure.Data.Repositories
{
	public class StudentFlattenedRepository : BaseRepository<StudentFlattened, SchoolCosmosDbOptions>, 
		IStudentFlattenedRepository
	{
		public StudentFlattenedRepository(SchoolCosmosDbOptions options) : base(options)
		{
		}

		public async Task<StudentFlattened> GetAsync(object id, object partitionKey, CancellationToken cancellationToken = default)
		{
			try
			{
				var requestOptions = new RequestOptions { PartitionKey = new PartitionKey(partitionKey) };
				var document = await DocumentClient.ReadDocumentAsync<StudentFlattened>(GetDocumentUri(id.ToString()),
					requestOptions, cancellationToken);
				return document.Document;
			}
			catch (DocumentClientException e)
			{
				if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					return null;
				}
				throw;
			}
		}
	}
}
