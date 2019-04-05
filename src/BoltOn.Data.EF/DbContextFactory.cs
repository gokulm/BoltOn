using System;
using BoltOn.Data.EF.Mediator;
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
		readonly MediatorDataContext _mediatorDataContext;

		public DbContextFactory(IServiceProvider serviceProvider,
			MediatorDataContext mediatorDataContext)
		{
			_mediatorDataContext = mediatorDataContext;
			_serviceProvider = serviceProvider;
		}

		public TDbContext Get<TDbContext>() where TDbContext : DbContext
		{
			var dbContext = _serviceProvider.GetService(typeof(TDbContext)) as TDbContext;
			if (_mediatorDataContext.IsQueryRequest)
			{
				dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
				dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
			}
			return dbContext;
		}
	}
}
