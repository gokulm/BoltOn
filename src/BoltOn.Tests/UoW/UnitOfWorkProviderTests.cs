using System;
using System.Transactions;
using BoltOn.Logging;
using BoltOn.UoW;
using Moq;
using Xunit;

namespace BoltOn.Tests.UoW
{
	public class UnitOfWorkProviderTests : IDisposable
	{
		[Fact]
		public void Start_WithDefaults_StartsTransactionWithDefaultAndReturnsUnitOfWork()
		{
			// arrange
			var loggerFactory = new Mock<IBoltOnLoggerFactory>();
			var uowProviderLogger = new Mock<IBoltOnLogger<UnitOfWorkProvider>>();
			loggerFactory.Setup(s => s.Create<UnitOfWorkProvider>()).Returns(uowProviderLogger.Object);
			var uow = new Mock<IBoltOnLogger<UnitOfWork>>();
			loggerFactory.Setup(s => s.Create<UnitOfWork>()).Returns(uow.Object);
			var unitOfWorkOptions = new UnitOfWorkOptions();
			var sut = new UnitOfWorkProvider(loggerFactory.Object);


			// act
			var result = sut.Start(unitOfWorkOptions);

			// assert
			var uowProviderLoggerStmt = $"Instantiating new UoW. IsolationLevel: {IsolationLevel.Serializable} " +
						  $"TransactionTimeOut: {TransactionManager.DefaultTimeout}" +
						  $"TransactionScopeOption: {TransactionScopeOption.Required}";
			var uowLoggerStmt = $"Starting UoW. IsolationLevel: {IsolationLevel.Serializable} " +
						  $"TransactionTimeOut: {TransactionManager.DefaultTimeout}" +
						  $"TransactionScopeOption: {TransactionScopeOption.Required}";
			uowProviderLogger.Verify(l => l.Debug(uowProviderLoggerStmt));
			uow.Verify(l => l.Debug(uowLoggerStmt));
			uow.Verify(l => l.Debug("Started UoW"));
		}

		public void Dispose()
		{
		}
	}
}
