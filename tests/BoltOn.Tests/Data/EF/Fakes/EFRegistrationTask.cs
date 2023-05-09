using BoltOn.Bootstrapping;
using BoltOn.Data;
using BoltOn.Data.EF;
using BoltOn.Tests.Other;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.DataAbstractions.EF;

namespace BoltOn.Tests.Data.EF.Fakes
{
	public static class EFRegistrationTask
	{
		public static void RegisterDataFakes(this BootstrapperOptions bootstrapperOptions)
		{
			bootstrapperOptions.ServiceCollection.AddTransient<IRepository<Student>, Repository<Student, SchoolDbContext>>();
			bootstrapperOptions.ServiceCollection.AddTransient<IQueryRepository<Student>, QueryRepository<Student, SchoolDbContext>>();

			if (IntegrationTestHelper.IsSqlServer)
			{
				bootstrapperOptions.ServiceCollection.AddDbContext<SchoolDbContext>(options =>
				{
					options.UseSqlServer("Data Source=127.0.0.1;initial catalog=Testing;persist security info=True;User ID=sa;Password=Password1;");
				});
			}
			else
			{
				bootstrapperOptions.ServiceCollection.AddDbContext<SchoolDbContext>(options =>
				{
					options.UseInMemoryDatabase("InMemoryDbForTesting");
				});
			}
		}
	}
}
