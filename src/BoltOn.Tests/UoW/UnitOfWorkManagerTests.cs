using System;
using System.Transactions;
using BoltOn.Logging;
using BoltOn.UoW;
using Moq;
using Xunit;

namespace BoltOn.Tests.UoW
{
	public class UnitOfWorkManagerTests : IDisposable
	{
		[Fact]
		public void Get_WithDefaults_StartsTransactionWithDefaultAndReturnsUnitOfWork()
		{
			// arrange
			var loggerFactory = new Mock<IBoltOnLoggerFactory>();
			var uowProviderLogger = new Mock<IBoltOnLogger<UnitOfWorkManager>>();
			loggerFactory.Setup(s => s.Create<UnitOfWorkManager>()).Returns(uowProviderLogger.Object);
			var uow = new Mock<IBoltOnLogger<UnitOfWork>>();
			loggerFactory.Setup(s => s.Create<UnitOfWork>()).Returns(uow.Object);
			var unitOfWorkOptions = new UnitOfWorkOptions();
			var sut = new UnitOfWorkManager(loggerFactory.Object);

			// act
			var result = sut.Get(unitOfWorkOptions);

			// assert
			var uowProviderLoggerStmt = $"Instantiating new UoW. IsolationLevel: {IsolationLevel.Serializable} " +
						  $"TransactionTimeOut: {TransactionManager.DefaultTimeout}" +
						  $"TransactionScopeOption: {TransactionScopeOption.Required}";
			uowProviderLogger.Verify(l => l.Debug(uowProviderLoggerStmt));
		}

		[Fact]
		public void Get_WithCustomIsolationLevel_StartsTransactionWithSpecifiedIsolationLevelAndReturnsUnitOfWork()
		{
			// arrange
			var loggerFactory = new Mock<IBoltOnLoggerFactory>();
			var uowProviderLogger = new Mock<IBoltOnLogger<UnitOfWorkManager>>();
			loggerFactory.Setup(s => s.Create<UnitOfWorkManager>()).Returns(uowProviderLogger.Object);
			var uow = new Mock<IBoltOnLogger<UnitOfWork>>();
			loggerFactory.Setup(s => s.Create<UnitOfWork>()).Returns(uow.Object);
			var timeSpan = TimeSpan.FromSeconds(30);
			var unitOfWorkOptions = new UnitOfWorkOptions
			{
				IsolationLevel = IsolationLevel.ReadCommitted,
				TransactionTimeout = timeSpan
			};
			var sut = new UnitOfWorkManager(loggerFactory.Object);

			// act
			var result = sut.Get(unitOfWorkOptions);

			// assert
			var uowProviderLoggerStmt = $"Instantiating new UoW. IsolationLevel: {IsolationLevel.ReadCommitted} " +
						  $"TransactionTimeOut: {timeSpan}" +
						  $"TransactionScopeOption: {TransactionScopeOption.Required}";
			uowProviderLogger.Verify(l => l.Debug(uowProviderLoggerStmt));
		}

		[Fact]
		public void Get_WithDefaultsTwice_StartsTransactionWithDefaultsAndStartsAnotherNew()
		{
			// arrange
			var loggerFactory = new Mock<IBoltOnLoggerFactory>();
			var uowProviderLogger = new Mock<IBoltOnLogger<UnitOfWorkManager>>();
			loggerFactory.Setup(s => s.Create<UnitOfWorkManager>()).Returns(uowProviderLogger.Object);
			var uow = new Mock<IBoltOnLogger<UnitOfWork>>();
			loggerFactory.Setup(s => s.Create<UnitOfWork>()).Returns(uow.Object);
			var unitOfWorkOptions = new UnitOfWorkOptions();
			var sut = new UnitOfWorkManager(loggerFactory.Object);

			// act
			var result = sut.Get(unitOfWorkOptions);
			var result2 = sut.Get(unitOfWorkOptions);

			// assert
			var uowProviderLoggerStmt = $"Instantiating new UoW. IsolationLevel: {IsolationLevel.Serializable} " +
					  $"TransactionTimeOut: {TransactionManager.DefaultTimeout}" +
					  $"TransactionScopeOption: {TransactionScopeOption.Required}";
			var uowProviderLoggerStmt2 = $"Instantiating new UoW. IsolationLevel: {IsolationLevel.Serializable} " +
					  $"TransactionTimeOut: {TransactionManager.DefaultTimeout}" +
					  $"TransactionScopeOption: {TransactionScopeOption.RequiresNew}";
			uowProviderLogger.Verify(l => l.Debug(uowProviderLoggerStmt));
			uowProviderLogger.Verify(l => l.Debug(uowProviderLoggerStmt2));
		}

		[Fact]
		public void Get_WithCustomIsolationLevelSetUsingAction_StartsTransactionWithSpecifiedIsolationLevelAndReturnsUnitOfWork()
		{
			// arrange
			var loggerFactory = new Mock<IBoltOnLoggerFactory>();
			var uowProviderLogger = new Mock<IBoltOnLogger<UnitOfWorkManager>>();
			loggerFactory.Setup(s => s.Create<UnitOfWorkManager>()).Returns(uowProviderLogger.Object);
			var uow = new Mock<IBoltOnLogger<UnitOfWork>>();
			loggerFactory.Setup(s => s.Create<UnitOfWork>()).Returns(uow.Object);
			var timeSpan = TimeSpan.FromSeconds(30);
			var sut = new UnitOfWorkManager(loggerFactory.Object);

			// act
			var result = sut.Get(u =>
			{
				u.IsolationLevel = IsolationLevel.ReadCommitted;
				u.TransactionTimeout = timeSpan;
			});

			// assert
			var uowProviderLoggerStmt = $"Instantiating new UoW. IsolationLevel: {IsolationLevel.ReadCommitted} " +
						  $"TransactionTimeOut: {timeSpan}" +
						  $"TransactionScopeOption: {TransactionScopeOption.Required}";
			uowProviderLogger.Verify(l => l.Debug(uowProviderLoggerStmt));
		}

		public void Dispose()
		{
		}
	}
}
