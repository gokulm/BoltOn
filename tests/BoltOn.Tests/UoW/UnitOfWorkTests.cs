using System;
using BoltOn.Logging;
using BoltOn.UoW;
using Moq;
using Xunit;

namespace BoltOn.Tests.UoW
{
    public class UnitOfWorkTests : IDisposable
    {
        private IUnitOfWork _sut;

        [Fact]
        public void Instantiate_WithDefaults_StartsTransaction()
        {
            // arrange
            var loggerFactory = new Mock<IBoltOnLoggerFactory>();
            var logger = new Mock<IBoltOnLogger<UnitOfWork>>();
            var unitOfWorkOptions = new UnitOfWorkOptions();
            loggerFactory.Setup(u => u.Create<UnitOfWork>()).Returns(logger.Object);

            // act
            _sut = new UnitOfWork(loggerFactory.Object, unitOfWorkOptions);

            // assert
            logger.Verify(l => l.Debug("Starting UoW..."));
            logger.Verify(l => l.Debug("Started UoW"));
        }

        [Fact]
        public void InstantiateAndCommit_WithDefaults_StartsAndCommitsTransaction()
        {
            // arrange
            var loggerFactory = new Mock<IBoltOnLoggerFactory>();
            var logger = new Mock<IBoltOnLogger<UnitOfWork>>();
            var unitOfWorkOptions = new UnitOfWorkOptions();
            loggerFactory.Setup(u => u.Create<UnitOfWork>()).Returns(logger.Object);

            // act
            _sut = new UnitOfWork(loggerFactory.Object, unitOfWorkOptions);
            _sut.Commit();

            // assert
            logger.Verify(l => l.Debug("Starting UoW..."));
            logger.Verify(l => l.Debug("Started UoW"));
            logger.Verify(l => l.Debug("Committing UoW..."));
            logger.Verify(l => l.Debug("Committed UoW"));
        }

        [Fact]
        public void InstantiateAndCommitAndDispose_WithDefaults_StartsAndCommitsAndDisposesTransaction()
        {
            // arrange
            var loggerFactory = new Mock<IBoltOnLoggerFactory>();
            var logger = new Mock<IBoltOnLogger<UnitOfWork>>();
            var unitOfWorkOptions = new UnitOfWorkOptions();
            loggerFactory.Setup(u => u.Create<UnitOfWork>()).Returns(logger.Object);

            // act
            using (var sut = new UnitOfWork(loggerFactory.Object, unitOfWorkOptions))
            {
                sut.Commit();
            }

            // assert
            logger.Verify(l => l.Debug("Starting UoW..."));
            logger.Verify(l => l.Debug("Started UoW"));
            logger.Verify(l => l.Debug("Committing UoW..."));
            logger.Verify(l => l.Debug("Committed UoW"));
            logger.Verify(l => l.Debug("Disposing UoW..."));
            logger.Verify(l => l.Debug("Disposed UoW"));
        }

        public void Dispose()
        {
            _sut?.Dispose();
        }
    }
}
