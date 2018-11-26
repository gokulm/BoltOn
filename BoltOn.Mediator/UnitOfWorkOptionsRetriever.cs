using System;
using System.Transactions;
using BoltOn.Logging;

namespace BoltOn.Mediator
{
	public interface IUnitOfWorkOptionsRetriever
	{
		UnitOfWorkOptions Get<TResponse>(IRequest<TResponse> request);
	}

	public class UnitOfWorkOptionsRetriever : IUnitOfWorkOptionsRetriever
	{
		private readonly IBoltOnLogger<UnitOfWorkOptionsRetriever> _logger;

		public UnitOfWorkOptionsRetriever(IBoltOnLogger<UnitOfWorkOptionsRetriever> logger)
		{
			_logger = logger;
		}

		public UnitOfWorkOptions Get<TResponse>(IRequest<TResponse> request)
		{
			IsolationLevel isolationLevel;
			switch (request)
			{
				case ICommand<TResponse> c:
					_logger.Debug("Getting isolation level for Command");
					isolationLevel = IsolationLevel.ReadCommitted;
					break;
				case IQuery<TResponse> q:
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
