using System;
using System.Transactions;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Mediator.UoW
{
	public interface IUnitOfWorkOptionsBuilder
	{
		UnitOfWorkOptions Get<TResponse>(IRequest<TResponse> request) where TResponse : class;
	}

	public class UnitOfWorkOptionsBuilder : IUnitOfWorkOptionsBuilder
	{
		private readonly IBoltOnLogger<UnitOfWorkOptionsBuilder> _logger;

		public UnitOfWorkOptionsBuilder(IBoltOnLogger<UnitOfWorkOptionsBuilder> logger)
		{
			_logger = logger;
		}

		public UnitOfWorkOptions Get<TResponse>(IRequest<TResponse> request) where TResponse : class
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
