using Microsoft.Extensions.DependencyInjection;
using BoltOn.Bootstrapping;
using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore;
using BoltOn.Data;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BoltOn.Tests.Cqrs.Fakes
{
    public class CqrsRegistrationTask : IRegistrationTask
    {
        public void Run(RegistrationTaskContext context)
        {
            context.ServiceCollection.AddDbContext<CqrsDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbCqrsDbContext");
                options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
            });

            context.ServiceCollection.AddTransient<IRepository<Student>, Repository<Student, CqrsDbContext>>();
            context.ServiceCollection.AddTransient<IRepository<StudentFlattened>, Repository<StudentFlattened, CqrsDbContext>>();
        }
    }
}
