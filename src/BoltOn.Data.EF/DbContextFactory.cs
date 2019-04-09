using System;
using BoltOn.Mediator;
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
		private readonly MediatorContext _mediatorContext;

		public DbContextFactory(IServiceProvider serviceProvider,
			MediatorContext mediatorContext)
		{
			_mediatorContext = mediatorContext;
			_serviceProvider = serviceProvider;
		}

		public TDbContext Get<TDbContext>() where TDbContext : DbContext
		{
		    if(!(_serviceProvider.GetService(typeof(TDbContext)) is TDbContext dbContext))
                throw new Exception("DbContext is null");
			if(_mediatorContext.IsQueryRequest)
			{
				dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
				dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
			}
			return dbContext;
		}
	}
}
