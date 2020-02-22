using BoltOn.Bootstrapping;
using BoltOn.Data;
using BoltOn.Data.EF;
using BoltOn.Tests.Other;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Data.EF
{
    public static class EFRegistrationTask
    {
        public static void RegisterDataFakes(this BoltOnOptions boltOnOptions)
        {
            boltOnOptions.ServiceCollection.AddTransient<IRepository<Student>, Repository<Student, SchoolDbContext>>();
            if (IntegrationTestHelper.IsSqlServer)
            {
                boltOnOptions.ServiceCollection.AddDbContext<SchoolDbContext>(options =>
                {
                    options.UseSqlServer("Data Source=127.0.0.1;initial catalog=Testing;persist security info=True;User ID=sa;Password=Password1;");
                });
            }
            else
            {
                boltOnOptions.ServiceCollection.AddDbContext<SchoolDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                    options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
                });
            }
        }
    }
}
