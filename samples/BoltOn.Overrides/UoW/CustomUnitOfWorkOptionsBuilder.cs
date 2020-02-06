using System;
using System.Transactions;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using BoltOn.Overrides.Mediator;
using BoltOn.UoW;

namespace BoltOn.Overrides.UoW
{
	public class CustomUnitOfWorkOptionsBuilder : IUnitOfWorkOptionsBuilder
	{
		private readonly IBoltOnLogger<CustomUnitOfWorkOptionsBuilder> _logger;

		public CustomUnitOfWorkOptionsBuilder(IBoltOnLogger<CustomUnitOfWorkOptionsBuilder> logger)
		{
			_logger = logger;
		}

		public virtual UnitOfWorkOptions Build<TResponse>(IRequest<TResponse> request) 
		{
			IsolationLevel isolationLevel;
			switch (request)
			{
				case ICommand<TResponse> _:
				case ICommand _:
					_logger.Debug("Getting isolation level for Command");
					isolationLevel = IsolationLevel.ReadCommitted;
					break;
				case IQuery<TResponse> _:
					_logger.Debug("Getting isolation level for Query");
					isolationLevel = IsolationLevel.ReadCommitted;
					break;
				case IQueryUncommitted<TResponse> _:
					_logger.Debug("Getting isolation level for StaleQuery");
					isolationLevel = IsolationLevel.ReadUncommitted;
					break;
				default:
					throw new Exception("Request should implement ICommand<> or IQuery<> to enable Unit of Work.");
			}
			return new UnitOfWorkOptions { IsolationLevel = isolationLevel };
		}
	}
}
