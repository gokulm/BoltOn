using System;
using BoltOn.Data.EF;
using BoltOn.Mediator.UoW;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Mediator.Data
{
	public class MediatorDbContextFactory : IDbContextFactory
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly IUnitOfWorkOptionsBuilder _unitOfWorkOptionsBuilder;

		public MediatorDbContextFactory(IServiceProvider serviceProvider,
			IUnitOfWorkOptionsBuilder unitOfWorkOptionsBuilder)
		{
			_serviceProvider = serviceProvider;
			_unitOfWorkOptionsBuilder = unitOfWorkOptionsBuilder;
		}

		public TDbContext Get<TDbContext>() where TDbContext : DbContext
		{
			var dbContext = _serviceProvider.GetService(typeof(TDbContext)) as TDbContext;
			if (_unitOfWorkOptionsBuilder.RequestType == Pipeline.RequestType.Query)
				dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
			return dbContext;
		}
	}
}
