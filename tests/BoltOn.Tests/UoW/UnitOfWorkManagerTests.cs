using System;
using System.Transactions;
using BoltOn.Logging;
using BoltOn.UoW;
using Moq;
using Xunit;

namespace BoltOn.Tests.UoW
{
	public class UnitOfWorkManagerTests
	{
		[Fact]
		public void Get_WithoutUnitOfWorkOptions_StartsTransactionWithDefaultAndReturnsUnitOfWork()
		{
			// arrange
			var uowManagerLogger = new Mock<IBoltOnLogger<UnitOfWorkManager>>();
			var uow = new Mock<IUnitOfWork>();
			var uowFactory = new Mock<IUnitOfWorkFactory>();
			uowFactory.Setup(u => u.Create(It.IsAny<UnitOfWorkOptions>())).Returns(uow.Object);
			var sut = new UnitOfWorkManager(uowManagerLogger.Object, uowFactory.Object);

			// act
			var result = sut.Get();

			// assert
			var uowProviderLoggerStmt = $"About to start UoW. IsolationLevel: {IsolationLevel.ReadCommitted} " +
						  $"TransactionTimeOut: {TransactionManager.DefaultTimeout}" +
						  $"TransactionScopeOption: {TransactionScopeOption.RequiresNew}";
			uowManagerLogger.Verify(l => l.Debug(uowProviderLoggerStmt));

			// cleanup
			result.Dispose();
		}

		[Fact]
		public void Get_WithDefaults_StartsTransactionWithDefaultAndReturnsUnitOfWork()
		{
			// arrange
			var uowManagerLogger = new Mock<IBoltOnLogger<UnitOfWorkManager>>();
			var uow = new Mock<IUnitOfWork>();
			var uowFactory = new Mock<IUnitOfWorkFactory>();
			var unitOfWorkOptions = new UnitOfWorkOptions();
			uowFactory.Setup(u => u.Create(unitOfWorkOptions)).Returns(uow.Object);
			var sut = new UnitOfWorkManager(uowManagerLogger.Object, uowFactory.Object);

			// act
			var result = sut.Get(unitOfWorkOptions);

			// assert
			var uowProviderLoggerStmt = $"About to start UoW. IsolationLevel: {IsolationLevel.ReadCommitted} " +
						  $"TransactionTimeOut: {TransactionManager.DefaultTimeout}" +
						  $"TransactionScopeOption: {TransactionScopeOption.RequiresNew}";
			uowManagerLogger.Verify(l => l.Debug(uowProviderLoggerStmt));

			// cleanup
			result.Dispose();
		}

		[Fact]
		public void Get_WithCustomIsolationLevel_StartsTransactionWithSpecifiedIsolationLevelAndReturnsUnitOfWork()
		{
			// arrange
			var uowManagerLogger = new Mock<IBoltOnLogger<UnitOfWorkManager>>();
			var uow = new Mock<IUnitOfWork>();
			var uowFactory = new Mock<IUnitOfWorkFactory>();
			var timeSpan = TimeSpan.FromSeconds(30);
			var unitOfWorkOptions = new UnitOfWorkOptions
			{
				IsolationLevel = IsolationLevel.ReadCommitted,
				TransactionTimeout = timeSpan
			};
			uowFactory.Setup(u => u.Create(unitOfWorkOptions)).Returns(uow.Object);
			var sut = new UnitOfWorkManager(uowManagerLogger.Object, uowFactory.Object);

			// act
			var result = sut.Get(unitOfWorkOptions);

			// assert
			var uowProviderLoggerStmt = $"About to start UoW. IsolationLevel: {IsolationLevel.ReadCommitted} " +
						  $"TransactionTimeOut: {timeSpan}" +
						  $"TransactionScopeOption: {TransactionScopeOption.RequiresNew}";
			uowManagerLogger.Verify(l => l.Debug(uowProviderLoggerStmt));      

			// cleanup
			result.Dispose();
		}

		[Fact]
		public void Get_WithDefaultsTwiceButSecondOneRequiresNew_StartsTransactionWithDefaultsAndANewOne()
		{
			// arrange
			var uowManagerLogger = new Mock<IBoltOnLogger<UnitOfWorkManager>>();
			var uow = new Mock<IUnitOfWork>();
			var uowFactory = new Mock<IUnitOfWorkFactory>();
			var unitOfWorkOptions = new UnitOfWorkOptions();
			uowFactory.Setup(u => u.Create(unitOfWorkOptions)).Returns(uow.Object);
			var sut = new UnitOfWorkManager(uowManagerLogger.Object, uowFactory.Object);

			// act
			var result = sut.Get(unitOfWorkOptions);
			var result2 = sut.Get(unitOfWorkOptions);

			// assert
			var uowProviderLoggerStmt = $"About to start UoW. IsolationLevel: {IsolationLevel.ReadCommitted} " +
					  $"TransactionTimeOut: {TransactionManager.DefaultTimeout}" +
					  $"TransactionScopeOption: {TransactionScopeOption.RequiresNew}";
			var uowProviderLoggerStmt2 = $"About to start UoW. IsolationLevel: {IsolationLevel.ReadCommitted} " +
					  $"TransactionTimeOut: {TransactionManager.DefaultTimeout}" +
					  $"TransactionScopeOption: {TransactionScopeOption.RequiresNew}";
			uowManagerLogger.Verify(l => l.Debug(uowProviderLoggerStmt));
			uowManagerLogger.Verify(l => l.Debug(uowProviderLoggerStmt2));

			// cleanup
			result.Dispose();
			result2.Dispose();
		}

        [Fact]
        public void Get_WithDefaultsTwiceButSecondOneDoesNotRequiresNew_StartsTransactionWithDefaultsAndAnExistingOne()
        {
            // arrange
            var uowManagerLogger = new Mock<IBoltOnLogger<UnitOfWorkManager>>();
            var uow = new Mock<IUnitOfWork>();
            var uowFactory = new Mock<IUnitOfWorkFactory>();
            var unitOfWorkOptions = new UnitOfWorkOptions();
            uowFactory.Setup(u => u.Create(unitOfWorkOptions)).Returns(uow.Object);
            var sut = new UnitOfWorkManager(uowManagerLogger.Object, uowFactory.Object);

            // act
            var result = sut.Get(unitOfWorkOptions);
            unitOfWorkOptions.TransactionScopeOption = TransactionScopeOption.Required;
            var result2 = sut.Get(unitOfWorkOptions);

            // assert
            var uowProviderLoggerStmt = $"About to start UoW. IsolationLevel: {IsolationLevel.ReadCommitted} " +
                                        $"TransactionTimeOut: {TransactionManager.DefaultTimeout}" +
                                        $"TransactionScopeOption: {TransactionScopeOption.RequiresNew}";
            var uowProviderLoggerStmt2 = $"About to start UoW. IsolationLevel: {IsolationLevel.ReadCommitted} " +
                                         $"TransactionTimeOut: {TransactionManager.DefaultTimeout}" +
                                         $"TransactionScopeOption: {TransactionScopeOption.Required}";
            uowManagerLogger.Verify(l => l.Debug(uowProviderLoggerStmt));
            uowManagerLogger.Verify(l => l.Debug(uowProviderLoggerStmt2));

            // cleanup
            result.Dispose();
            result2.Dispose();
        }

        [Fact]
		public void Get_WithCustomIsolationLevelSetUsingAction_StartsTransactionWithSpecifiedIsolationLevelAndReturnsUnitOfWork()
		{
			// arrange
			var uowManagerLogger = new Mock<IBoltOnLogger<UnitOfWorkManager>>();
			var uowFactory = new Mock<IUnitOfWorkFactory>();
			var timeSpan = TimeSpan.FromSeconds(30);
			var sut = new UnitOfWorkManager(uowManagerLogger.Object, uowFactory.Object);

			// act
			sut.Get(u =>
			{
			    u.IsolationLevel = IsolationLevel.ReadCommitted;
			    u.TransactionTimeout = timeSpan;
			});

			// assert
			var uowProviderLoggerStmt = $"About to start UoW. IsolationLevel: {IsolationLevel.ReadCommitted} " +
						  $"TransactionTimeOut: {timeSpan}" +
						  $"TransactionScopeOption: {TransactionScopeOption.RequiresNew}";
			uowManagerLogger.Verify(l => l.Debug(uowProviderLoggerStmt));
		}
	}
}
