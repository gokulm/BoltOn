using System;
using System.Transactions;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using BoltOn.Mediator.UoW;
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
                case ICommand<TResponse> c:
                case IQuery<TResponse> q:
                    _logger.Debug("Getting isolation level for Command or Query");
                    isolationLevel = IsolationLevel.ReadCommitted;
                    break;
                default:
                    throw new Exception("Request should implement ICommand<> or IQuery<> to enable Unit of Work.");
            }
            return new UnitOfWorkOptions { IsolationLevel = isolationLevel };
        }
    }
}
