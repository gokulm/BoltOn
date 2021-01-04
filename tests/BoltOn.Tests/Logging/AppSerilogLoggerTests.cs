using System;
using System.Collections.Generic;
using BoltOn.Logging;
using BoltOn.Logging.Serilog;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace BoltOn.Tests.Logging
{
	public class AppSerilogLoggerTests
	{
		private readonly AutoMocker _autoMocker;
		private readonly AppSerilogLogger<TestClass> _sut;
		private readonly Mock<LoggerContext> _loggerContext;
		private readonly Mock<IConfiguration> _configuration;

		public AppSerilogLoggerTests()
		{
			// arrange
			_autoMocker = new AutoMocker();
			_sut = _autoMocker.CreateInstance<AppSerilogLogger<TestClass>>();
			var serviceProvider = _autoMocker.GetMock<IServiceProvider>();
			_loggerContext = new Mock<LoggerContext>();
			var keyValues = new Dictionary<string, object>();
			keyValues.Add("key1", "value1");
			_loggerContext.Setup(s => s.Keys).Returns(keyValues.Keys);
			serviceProvider.Setup(s => s.GetService(typeof(LoggerContext))).Returns(_loggerContext.Object);

			_configuration = _autoMocker.GetMock<IConfiguration>();
			_configuration.Setup(s => s.GetSection("Module")).Returns(Mock.Of<IConfigurationSection>());

		}

		[Fact]
		public void Debug_WithMessage_PushesLoggerContextValuesToTheContextAndLogs()
		{
			// act
			_sut.Debug("test");

			// assert
			Assert();
		}

		[Fact]
		public void Info_WithMessage_PushesLoggerContextValuesToTheContextAndLogs()
		{
			// act
			_sut.Info("test");

			// assert
			Assert();
		}

		[Fact]
		public void Warn_WithMessage_PushesLoggerContextValuesToTheContextAndLogs()
		{
			// act
			_sut.Warn("test");

			// assert
			Assert();
		}

		[Fact]
		public void Error_WithMessage_PushesLoggerContextValuesToTheContextAndLogs()
		{
			// act
			_sut.Error("test");

			// assert
			Assert();
		}

		[Fact]
		public void Error_WithException_PushesLoggerContextValuesToTheContextAndLogs()
		{
			// act
			_sut.Error(new Exception("test"));

			// assert
			Assert();
		}

		private void Assert()
		{
			_loggerContext.Verify(v => v.Keys);
			_loggerContext.Verify(v => v.GetByKey<string>("key1", null));
			_configuration.Verify(v => v.GetSection("Module"));
		}
	}

	public class TestClass
	{
	}
}
