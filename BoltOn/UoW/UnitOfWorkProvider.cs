using System.Transactions;

namespace BoltOn.UoW
{
	public interface IUnitOfWorkProvider
	{
		IUnitOfWork Get(IsolationLevel isolationLevel);
	}

	public class UnitOfWorkProvider : IUnitOfWorkProvider
	{
		private IUnitOfWork _unitOfWork;

		public IUnitOfWork Get(IsolationLevel isolationLevel)
		{
			return _unitOfWork ?? (_unitOfWork = new TestUnitOfWork(isolationLevel));
		}
	}
}
