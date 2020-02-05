using Microsoft.Extensions.DependencyInjection;
using BoltOn.Samples.Infrastructure.Data;
using BoltOn.Data;
using BoltOn.Samples.Application.Entities;
using BoltOn.Bootstrapping;

namespace BoltOn.Samples.WebApi
{
    public class RegistrationTask : IRegistrationTask
    {
        public void Run(RegistrationTaskContext context)
        {
            var serviceCollection = context.ServiceCollection;
			serviceCollection.AddTransient<IRepository<Student>, Data.EF.Repository<Student, SchoolWriteDbContext>>();
			serviceCollection.AddTransient<IRepository<StudentType>, Data.EF.Repository<StudentType, SchoolWriteDbContext>>();
			serviceCollection.AddTransient<IRepository<StudentFlattened>, Data.EF.Repository<StudentFlattened, SchoolReadDbContext>>();
		}
    }
}
