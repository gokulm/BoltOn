using System.Collections.Generic;
using BoltOn.Logging;
using BoltOn.Exceptions;
using BoltOn.Web.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace BoltOn.Tests.Web
{
	public class ModelValidationFilterTests
	{
		[Fact]
		public void OnException_ValidModel_DoesNotThrowModelValidationException()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<ModelValidationFilter>();
			var logger = autoMocker.GetMock<IAppLogger<ModelValidationFilter>>();
			var modelState = new ModelStateDictionary();

			var actionContext = new ActionContext(
				Mock.Of<HttpContext>(),
				Mock.Of<RouteData>(),
				Mock.Of<ActionDescriptor>(),
				Mock.Of<ModelStateDictionary>()
			);

			var actionExecutingContext = new ActionExecutingContext(
				actionContext,
				new List<IFilterMetadata>(),
				new Dictionary<string, object>(),
				Mock.Of<Controller>()
			);

			// act
			sut.OnActionExecuting(actionExecutingContext);

			// assert
			logger.Verify(v => v.Debug("Model is invalid"), Times.Never);
		}

		[Fact]
		public void OnException_ModelWithOneValidationError_ThrowsBadRequestException()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<ModelValidationFilter>();
			var logger = autoMocker.GetMock<IAppLogger<ModelValidationFilter>>();
			var modelState = new ModelStateDictionary();
			modelState.AddModelError("name", "invalid name");

			var actionContext = new ActionContext(
				Mock.Of<HttpContext>(),
				Mock.Of<RouteData>(),
				Mock.Of<ActionDescriptor>(),
				modelState
			);

			var actionExecutingContext = new ActionExecutingContext(
				actionContext,
				new List<IFilterMetadata>(),
				new Dictionary<string, object>(),
				Mock.Of<Controller>()
			);

			// act
			var exception = Record.Exception(() => sut.OnActionExecuting(actionExecutingContext));

			// assert
			logger.Verify(v => v.Debug("Model is invalid"));
			Assert.NotNull(exception);
			Assert.IsType<BadRequestException>(exception);
			Assert.Equal("invalid name", exception.Message);
		}

		[Fact]
		public void OnException_ModelWithMoreThanOneValidationError_ThrowsBadRequestExceptionWithCommaSeparatedValidationMessage()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<ModelValidationFilter>();
			var logger = autoMocker.GetMock<IAppLogger<ModelValidationFilter>>();
			var modelState = new ModelStateDictionary();
			modelState.AddModelError("firstName", "invalid first name");
			modelState.AddModelError("lastName", "invalid last name");

			var actionContext = new ActionContext(
				Mock.Of<HttpContext>(),
				Mock.Of<RouteData>(),
				Mock.Of<ActionDescriptor>(),
				modelState
			);

			var actionExecutingContext = new ActionExecutingContext(
				actionContext,
				new List<IFilterMetadata>(),
				new Dictionary<string, object>(),
				Mock.Of<Controller>()
			);

			// act
			var exception = Record.Exception(() => sut.OnActionExecuting(actionExecutingContext));

			// assert
			logger.Verify(v => v.Debug("Model is invalid"));
			Assert.NotNull(exception);
			Assert.IsType<BadRequestException>(exception);
			Assert.Equal("invalid last name | invalid first name", exception.Message);
		}
	}
}
