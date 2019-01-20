using System;
using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Mediator.Data.EF
{
	public class MediatorDbContextFactory : IDbContextFactory
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly IMediatorDataContext _mediatorDataContext;

		public MediatorDbContextFactory(IServiceProvider serviceProvider,
			IMediatorDataContext mediatorDataContext)
		{
			_serviceProvider = serviceProvider;
			_mediatorDataContext = mediatorDataContext;
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
