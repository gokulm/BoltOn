using BoltOn.Bootstrapping;
using BoltOn.Tests.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Data.EF
{
	public class TestDataRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			if (MediatorTestHelper.IsSqlite)
			{
				context.Container.AddDbContext<SchoolDbContext>(options =>
				{
					options.UseSqlite("Data Source=TestDatabase.db");
					options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
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
