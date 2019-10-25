using System.Threading;
using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Abstractions.Data
{
	public interface IStudentFlattenedRepository : IRepository<StudentFlattened>
	{
		Task<StudentFlattened> GetAsync(object id, object partitionKey, CancellationToken cancellationToken);
	}
}
