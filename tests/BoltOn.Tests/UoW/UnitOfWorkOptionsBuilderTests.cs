using BoltOn.Logging;
using BoltOn.Tests.Mediator;
using BoltOn.UoW;
using Moq;
using Xunit;

namespace BoltOn.Tests.UoW
{
	public class UnitOfWorkOptionsBuilderTests
	{
		[Fact]
		public void Build_CommandRequest_ReturnsUoWOptionsBuildWithProperIsolationLevel()
		{
			// arrange
			var loggerFactory = new Mock<IBoltOnLoggerFactory>();
			var logger = new Mock<IBoltOnLogger<UnitOfWorkOptionsBuilder>>();
			loggerFactory.Setup(u => u.Create<UnitOfWorkOptionsBuilder>()).Returns(logger.Object);
			var sut = new UnitOfWorkOptionsBuilder(logger.Object);

			// act
			var result = sut.Build(new TestCommand());

			// assert
			Assert.Equal(System.Transactions.IsolationLevel.ReadCommitted, result.IsolationLevel);
			logger.Verify(l => l.Debug("Getting isolation level for Command"));
		}

		[Fact]
		public void Build_QueryRequest_ReturnsUoWOptionsBuildWithProperIsolationLevel()
		{
			// arrange
			var loggerFactory = new Mock<IBoltOnLoggerFactory>();
			var logger = new Mock<IBoltOnLogger<UnitOfWorkOptionsBuilder>>();
			loggerFactory.Setup(u => u.Create<UnitOfWorkOptionsBuilder>()).Returns(logger.Object);
			var sut = new UnitOfWorkOptionsBuilder(logger.Object);

			// act
			var result = sut.Build(new TestQuery());

			// assert
			Assert.Equal(System.Transactions.IsolationLevel.ReadCommitted, result.IsolationLevel);
			logger.Verify(l => l.Debug("Getting isolation level for Query"));
		}

		[Fact]
		public void Build_StaleQueryRequest_ReturnsUoWOptionsBuildWithProperIsolationLevel()
		{
			// arrange
			var loggerFactory = new Mock<IBoltOnLoggerFactory>();
			var logger = new Mock<IBoltOnLogger<UnitOfWorkOptionsBuilder>>();
			loggerFactory.Setup(u => u.Create<UnitOfWorkOptionsBuilder>()).Returns(logger.Object);
			var sut = new UnitOfWorkOptionsBuilder(logger.Object);

			// act
			var result = sut.Build(new TestStaleQuery());

			// assert
			Assert.Equal(System.Transactions.IsolationLevel.ReadUncommitted, result.IsolationLevel);
			logger.Verify(l => l.Debug("Getting isolation level for StaleQuery"));
		}
	}
}
