using System;
using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Data.CosmosDb;
using Nito.AsyncEx;

namespace BoltOn.Tests.Other
{
    public interface IGradeRepository : IRepository<Grade>
    {
        void DeleteById<TId>(TId id);
        Task DeleteByIdAsync<TId>(TId id);
    }

    public class GradeRepository : BaseCosmosDbRepository<Grade, CollegeDbContext>, IGradeRepository
    {
        public GradeRepository(CollegeDbContext collegeDbContext) : base(collegeDbContext)
        {
        }

        public virtual void DeleteById<TId>(TId id)
        {
            AsyncContext.Run(() => DocumentClient.DeleteDocumentAsync(GetDocumentUri(id.ToString())));
        }

        public virtual async Task DeleteByIdAsync<TId>(TId id)
        {
            await DocumentClient.DeleteDocumentAsync(GetDocumentUri(id.ToString()));
        }
    }
}
