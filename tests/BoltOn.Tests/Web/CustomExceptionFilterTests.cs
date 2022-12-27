using System;
using System.Collections.Generic;
using BoltOn.Logging;
using BoltOn.Exceptions;
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
		public void OnException_ExceptionAlreadyHandled_ReturnsWithoutCustomHandling()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<CustomExceptionFilter>();
			var logger = autoMocker.GetMock<IAppLogger<CustomExceptionFilter>>();
			var actionContext = new ActionContext()
			{
				HttpContext = Mock.Of<HttpContext>(),
				RouteData = Mock.Of<RouteData>(),
				ActionDescriptor = Mock.Of<ActionDescriptor>()
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
			var logger = autoMocker.GetMock<IAppLogger<CustomExceptionFilter>>();
			var actionContext = new ActionContext()
			{
				HttpContext = Mock.Of<HttpContext>(),
				RouteData = Mock.Of<RouteData>(),
				ActionDescriptor = Mock.Of<ActionDescriptor>()
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
			var logger = autoMocker.GetMock<IAppLogger<CustomExceptionFilter>>();

			var configuration = autoMocker.GetMock<IConfiguration>();
			configuration.Setup(s => s.GetSection(It.IsAny<string>())).Returns(Mock.Of<IConfigurationSection>());

			var corrleationContextAccessor = autoMocker.GetMock<ICorrelationContextAccessor>();
			var correlationId = Guid.NewGuid().ToString();
			var correlationContext = new CorrelationContext(correlationId, "test header");
			corrleationContextAccessor.Setup(s => s.CorrelationContext).Returns(correlationContext);

			var actionContext = new ActionContext()
			{
				HttpContext = new DefaultHttpContext(),
				RouteData = Mock.Of<RouteData>(),
				ActionDescriptor = Mock.Of<ActionDescriptor>()
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
		public void OnException_BusinessValidationExceptionThrownWithContentTypeJson_Returns412AndJsonResult()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<CustomExceptionFilter>();
			var logger = autoMocker.GetMock<IAppLogger<CustomExceptionFilter>>();

			var configuration = autoMocker.GetMock<IConfiguration>();
			configuration.Setup(s => s.GetSection(It.IsAny<string>())).Returns(Mock.Of<IConfigurationSection>());

			var corrleationContextAccessor = autoMocker.GetMock<ICorrelationContextAccessor>();
			var correlationId = Guid.NewGuid().ToString();
			var correlationContext = new CorrelationContext(correlationId, "test header");
			corrleationContextAccessor.Setup(s => s.CorrelationContext).Returns(correlationContext);

			var actionContext = new ActionContext()
			{
				HttpContext = new DefaultHttpContext(),
				RouteData = Mock.Of<RouteData>(),
				ActionDescriptor = Mock.Of<ActionDescriptor>()
			};
			actionContext.HttpContext.Request.ContentType = "application/json";
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
			var jsonResult = (JsonResult)exceptionContext.Result;
			Assert.NotNull(jsonResult);
			var errorModel = (ErrorModel)jsonResult.Value;
			Assert.Equal("test 412", errorModel.Message);
			Assert.Equal(correlationId, errorModel.Id);
		}
	}
}
