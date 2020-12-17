using System;
using System.Collections.Generic;
using BoltOn.Logging;
using BoltOn.Web.Exceptions;
using BoltOn.Web.Filters;
using BoltOn.Web.Models;
using CorrelationId;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace BoltOn.Tests.Web
{
	public class CustomExceptionFilterTests
	{
		[Fact]
		public void OnException_ExceptionHandled_ReturnsWithoutCustomHandling()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<CustomExceptionFilter>();
			var logger = autoMocker.GetMock<IBoltOnLogger<CustomExceptionFilter>>();
			var actionContext = new ActionContext()
			{
				HttpContext = new DefaultHttpContext(),
				RouteData = new RouteData(),
				ActionDescriptor = new ActionDescriptor()
			};
			var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
			{
				ExceptionHandled = true
			};

			// act
			sut.OnException(exceptionContext);

			// assert
			logger.Verify(v => v.Debug("Exception already handled"));
		}

		[Fact]
		public void OnException_ExceptionIsNull_ReturnsWithoutCustomHandling()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<CustomExceptionFilter>();
			var logger = autoMocker.GetMock<IBoltOnLogger<CustomExceptionFilter>>();
			var actionContext = new ActionContext()
			{
				HttpContext = new DefaultHttpContext(),
				RouteData = new RouteData(),
				ActionDescriptor = new ActionDescriptor()
			};
			var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>());

			// act
			sut.OnException(exceptionContext);

			// assert
			logger.Verify(v => v.Debug("Exception is null"));
		}

		[Fact]
		public void OnException_BusinessValidationExceptionThrown_Returns412AndViewResult()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<CustomExceptionFilter>();
			var logger = autoMocker.GetMock<IBoltOnLogger<CustomExceptionFilter>>();

			var configuration = autoMocker.GetMock<IConfiguration>();
			configuration.Setup(s => s.GetSection(It.IsAny<string>())).Returns(new Mock<IConfigurationSection>().Object);

			var corrleationContextAccessor = autoMocker.GetMock<ICorrelationContextAccessor>();
			var correlationId = Guid.NewGuid().ToString();
			var correlationContext = new CorrelationContext(correlationId, "test header");
			corrleationContextAccessor.Setup(s => s.CorrelationContext).Returns(correlationContext);

			var actionContext = new ActionContext()
			{
				HttpContext = new DefaultHttpContext(),
				RouteData = new RouteData(),
				ActionDescriptor = new ActionDescriptor()
			};
			var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
			{
				Exception = new BusinessValidationException("test 412")
			};

			// act
			sut.OnException(exceptionContext);

			// assert
			logger.Verify(v => v.Warn("test 412"));
			Assert.Equal(412, exceptionContext.HttpContext.Response.StatusCode);
			Assert.True(exceptionContext.ExceptionHandled);
			Assert.NotNull(exceptionContext.Result);
			var viewResult = (ViewResult)exceptionContext.Result;
			Assert.NotNull(viewResult);
			Assert.Equal("Error", viewResult.ViewName);
			var errorModel = (ErrorModel)viewResult.ViewData.Model;
			Assert.Equal("test 412", errorModel.Message);
			Assert.Equal(correlationId, errorModel.Id);
		}

		[Fact]
		public void OnException_BadRequestExceptionThrown_Returns400AndViewResult()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<CustomExceptionFilter>();
			var logger = autoMocker.GetMock<IBoltOnLogger<CustomExceptionFilter>>();

			var configuration = autoMocker.GetMock<IConfiguration>();
			configuration.Setup(s => s.GetSection(It.IsAny<string>())).Returns(new Mock<IConfigurationSection>().Object);

			var corrleationContextAccessor = autoMocker.GetMock<ICorrelationContextAccessor>();
			var correlationId = Guid.NewGuid().ToString();
			var correlationContext = new CorrelationContext(correlationId, "test header");
			corrleationContextAccessor.Setup(s => s.CorrelationContext).Returns(correlationContext);

			var actionContext = new ActionContext()
			{
				HttpContext = new DefaultHttpContext(),
				RouteData = new RouteData(),
				ActionDescriptor = new ActionDescriptor()
			};
			var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
			{
				Exception = new BadRequestException("test 400")
			};

			// act
			sut.OnException(exceptionContext);

			// assert
			logger.Verify(v => v.Warn("test 400"));
			Assert.Equal(400, exceptionContext.HttpContext.Response.StatusCode);
			Assert.True(exceptionContext.ExceptionHandled);
			Assert.NotNull(exceptionContext.Result);
			var viewResult = (ViewResult)exceptionContext.Result;
			Assert.NotNull(viewResult);
			Assert.Equal("Error", viewResult.ViewName);
			var errorModel = (ErrorModel)viewResult.ViewData.Model;
			Assert.Equal("test 400", errorModel.Message);
			Assert.Equal(correlationId, errorModel.Id);
		}

		[Fact]
		public void OnException_ExceptionThrownWithIsShowErrorsFalse_Returns500AndViewResult()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<CustomExceptionFilter>();
			var logger = autoMocker.GetMock<IBoltOnLogger<CustomExceptionFilter>>();

			var configuration = autoMocker.GetMock<IConfiguration>();
			var configurationSection1 = new Mock<IConfigurationSection>();
			configuration.Setup(s => s.GetSection("IsShowErrors")).Returns(new Mock<IConfigurationSection>().Object);
			configuration.Setup(s => s.GetSection("ErrorMessage")).Returns(configurationSection1.Object);
			configurationSection1.Setup(s => s.Value).Returns("test generic message");
			var configurationSection2 = new Mock<IConfigurationSection>();
			configuration.Setup(s => s.GetSection("ErrorViewName")).Returns(configurationSection2.Object);
			configurationSection2.Setup(s => s.Value).Returns("ErrorView");

			var corrleationContextAccessor = autoMocker.GetMock<ICorrelationContextAccessor>();
			var correlationId = Guid.NewGuid().ToString();
			var correlationContext = new CorrelationContext(correlationId, "test header");
			corrleationContextAccessor.Setup(s => s.CorrelationContext).Returns(correlationContext);

			var actionContext = new ActionContext()
			{
				HttpContext = new DefaultHttpContext(),
				RouteData = new RouteData(),
				ActionDescriptor = new ActionDescriptor()
			};
			var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
			{
				Exception = new Exception("test 500")
			};

			// act
			sut.OnException(exceptionContext);

			// assert
			logger.Verify(v => v.Error(exceptionContext.Exception));
			Assert.Equal(500, exceptionContext.HttpContext.Response.StatusCode);
			Assert.True(exceptionContext.ExceptionHandled);
			Assert.NotNull(exceptionContext.Result);
			var viewResult = (ViewResult)exceptionContext.Result;
			Assert.NotNull(viewResult);
			Assert.Equal("ErrorView", viewResult.ViewName);
			var errorModel = (ErrorModel)viewResult.ViewData.Model;
			Assert.Equal("test generic message", errorModel.Message);
			Assert.Equal(correlationId, errorModel.Id);
		}
	}
}
