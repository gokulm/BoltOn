using BoltOn.Bootstrapping;
using BoltOn.Data;
using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Other
{
	public class TestRegistrationTask : IRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			RegisterDataFakes(context);
			context.Container.AddTransient<IRepository<Student>, Repository<Student, SchoolDbContext>>();
		}

		private static void RegisterDataFakes(RegistrationTaskContext context)
		{
			if (IntegrationTestHelper.IsSqlServer)
			{
				context.Container.AddDbContext<SchoolDbContext>(options =>
				{
					options.UseSqlServer("Data Source=127.0.0.1;initial catalog=Testing;persist security info=True;User ID=sa;Password=Password1;");
				});
			}
			else
			{
				context.Container.AddDbContext<SchoolDbContext>(options =>
				{
					options.UseInMemoryDatabase("InMemoryDbForTesting");
					options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));

				});
			}
		}
	}
}
