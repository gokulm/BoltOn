using Microsoft.Extensions.DependencyInjection;
using BoltOn.Bootstrapping;
using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore;
using BoltOn.Data;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BoltOn.Tests.Cqrs.Fakes
{
    public static class CqrsRegistrationTask
    {
        public static void RegisterCqrsFakes(this BootstrapperOptions bootstrapperOptions)
        {
            bootstrapperOptions.ServiceCollection.AddLogging();
            bootstrapperOptions.ServiceCollection.AddDbContext<CqrsDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbCqrsDbContext");
                options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
            });

            bootstrapperOptions.ServiceCollection.AddTransient<IRepository<Student>, 
                CqrsRepository<Student, CqrsDbContext>>();
            bootstrapperOptions.ServiceCollection.AddTransient<IRepository<StudentFlattened>,
                CqrsRepository<StudentFlattened, CqrsDbContext>>();
        }
    }
}
