using System;
using System.Transactions;

namespace BoltOn.UoW
{
	public interface IUnitOfWorkProvider
	{
		IUnitOfWork Get(IsolationLevel isolationLevel, TimeSpan? transactionTimeOut = null);
	}

	public class UnitOfWorkProvider : IUnitOfWorkProvider
	{
		private IUnitOfWork _unitOfWork;

		public IUnitOfWork Get(IsolationLevel isolationLevel, TimeSpan? transactionTimeOut = null)
		{
			return _unitOfWork ?? (_unitOfWork = new UnitOfWork(isolationLevel, transactionTimeOut ?? TransactionManager.DefaultTimeout));
		}
	}
}
