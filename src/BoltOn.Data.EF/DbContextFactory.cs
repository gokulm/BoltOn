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
		private readonly ChangeTrackerContext _changeTrackerContext;

		public DbContextFactory(IServiceProvider serviceProvider,
			ChangeTrackerContext changeTrackerContext)
		{
			_changeTrackerContext = changeTrackerContext;
			_serviceProvider = serviceProvider;
		}

		public TDbContext Get<TDbContext>() where TDbContext : DbContext
		{
		    if(!(_serviceProvider.GetService(typeof(TDbContext)) is TDbContext dbContext))
                throw new Exception("DbContext is null");

			if(_changeTrackerContext.IsQueryRequest)
			{
				dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
				dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
			}
			return dbContext;
		}
	}
}
