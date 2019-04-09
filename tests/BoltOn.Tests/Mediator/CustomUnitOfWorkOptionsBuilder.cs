using System;
using System.Transactions;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using BoltOn.Ovverides.Mediator;
using BoltOn.UoW;

namespace BoltOn.Tests.Mediator
{
	public class CustomUnitOfWorkOptionsBuilder : IUnitOfWorkOptionsBuilder
    {
        private readonly IBoltOnLogger<CustomUnitOfWorkOptionsBuilder> _logger;

        public CustomUnitOfWorkOptionsBuilder(IBoltOnLogger<CustomUnitOfWorkOptionsBuilder> logger)
        {
            _logger = logger;
        }

        public UnitOfWorkOptions Build<TResponse>(IRequest<TResponse> request)
        {
			IsolationLevel isolationLevel;
			switch (request)
            {
                case ICommand<TResponse> _:
                case IQuery<TResponse> _:
				case ICommand _:
					_logger.Debug("Getting isolation level for Command or Query");
                    isolationLevel = IsolationLevel.ReadCommitted;
                    break;
				case IQueryUncommitted<TResponse> _:
					_logger.Debug("Getting isolation level for QueryUncommitted");
					isolationLevel = IsolationLevel.ReadUncommitted;
					break;
				default:
                    throw new Exception("Request should implement ICommand<> or IQuery<> to enable Unit of Work.");
            }
            return new UnitOfWorkOptions { IsolationLevel = isolationLevel };
        }
    }
}
