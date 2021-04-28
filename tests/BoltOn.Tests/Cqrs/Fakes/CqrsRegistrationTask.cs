using BoltOn.Bootstrapping;
using BoltOn.Cqrs;
using BoltOn.Data;
using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Cqrs.Fakes
{
    public static class CqrsRegistrationTask
    {
        public static void RegisterCqrsFakes(this BootstrapperOptions bootstrapperOptions)
        {
            bootstrapperOptions.ServiceCollection.AddLogging();
            bootstrapperOptions.ServiceCollection.AddDbContext<SchoolDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbCqrsDbContext");
                options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
            });

            bootstrapperOptions.ServiceCollection.AddTransient<IRepository<Student>,
                CqrsRepository<Student, SchoolDbContext>>();
            bootstrapperOptions.ServiceCollection.AddTransient<IRepository<StudentFlattened>,
                Repository<StudentFlattened, SchoolDbContext>>();
            bootstrapperOptions.ServiceCollection.AddTransient<IRepository<EventStore>,
                Repository<EventStore, SchoolDbContext>>();
        }
    }
}
