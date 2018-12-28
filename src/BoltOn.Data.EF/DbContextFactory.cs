using System;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Data.EF
{
	public interface IDbContextFactory
	{
		TDbContext Get<TDbContext>() where TDbContext : DbContext;
	}

	public class DbContextFactory : IDbContextFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public DbContextFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public TDbContext Get<TDbContext>() where TDbContext : DbContext
		{
			var dbContext = _serviceProvider.GetService(typeof(TDbContext)) as TDbContext;
			return dbContext;
		}
	}
}
