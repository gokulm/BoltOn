using BoltOn.Bootstrapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Other
{
	public class TestDataRegistrationTask : IRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			if (MediatorTestHelper.IsSqlServer)
			{
				//context.Container.AddDbContext<SchoolDbContext>(options =>
				//{
				//	options.UseSqlite("Data Source=TestDatabase.db");
				//	options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
				//});


				context.Container.AddDbContext<SchoolDbContext>(options =>
				{
					options.UseSqlServer("Data Source=127.0.0.1;initial catalog=Testing;persist security info=True;User ID=sa;Password=$Password1;");
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
