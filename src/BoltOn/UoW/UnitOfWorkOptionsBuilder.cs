using System;
using System.Transactions;
using BoltOn.Logging;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.UoW
{
	public interface IUnitOfWorkOptionsBuilder
	{
		UnitOfWorkOptions Build<TResponse>(IRequest<TResponse> request);
	}

	public class UnitOfWorkOptionsBuilder : IUnitOfWorkOptionsBuilder
	{
		private readonly IBoltOnLogger<UnitOfWorkOptionsBuilder> _logger;

		public UnitOfWorkOptionsBuilder(IBoltOnLogger<UnitOfWorkOptionsBuilder> logger)
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
					isolationLevel = IsolationLevel.ReadUncommitted;
					break;
				default:
					throw new Exception("Request should implement ICommand<> or IQuery<> to enable Unit of Work.");
			}
			return new UnitOfWorkOptions { IsolationLevel = isolationLevel };
		}
	}
}
