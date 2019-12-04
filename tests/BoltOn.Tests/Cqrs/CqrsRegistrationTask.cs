using Microsoft.Extensions.DependencyInjection;
using BoltOn.Bootstrapping;
using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore;
using BoltOn.Data;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BoltOn.Tests.Cqrs
{
    public class CqrsRegistrationTask : IRegistrationTask
    {
        public void Run(RegistrationTaskContext context)
        {
            context.Container.AddDbContext<CqrsDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbCqrsDbContext");
                options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
            });

            context.Container.AddTransient<IRepository<Student>, Repository<Student, CqrsDbContext>>();
            context.Container.AddTransient<IRepository<StudentFlattened>, Repository<StudentFlattened, CqrsDbContext>>();
        }
    }
}
