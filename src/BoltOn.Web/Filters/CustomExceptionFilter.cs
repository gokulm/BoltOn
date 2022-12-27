using BoltOn.Logging;
using BoltOn.Exceptions;
using BoltOn.Web.Models;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace BoltOn.Web.Filters
{
	public class CustomExceptionFilter : ExceptionFilterAttribute
	{
		private readonly IConfiguration _configuration;
		private readonly ICorrelationContextAccessor _correlationContextAccessor;
		private readonly IAppLogger<CustomExceptionFilter> _logger;

		public CustomExceptionFilter(IAppLogger<CustomExceptionFilter> logger,
			IConfiguration configuration,
			ICorrelationContextAccessor correlationContextAccessor)
		{
			_configuration = configuration;
			_correlationContextAccessor = correlationContextAccessor;
			_logger = logger;
		}

		public override void OnException(ExceptionContext exceptionContext)
		{
			if (exceptionContext.ExceptionHandled)
			{
				_logger.Debug("Exception already handled");
				return;
			}

			var exception = exceptionContext.Exception;
			if (exception == null)
			{
				_logger.Debug("Exception is null");
				return;
			}

			var errorViewModel = BuildErrorModel(exceptionContext);
			if (exceptionContext.HttpContext != null &&
				exceptionContext.HttpContext.Request.ContentType != null &&
				exceptionContext.HttpContext.Request.ContentType.Contains("application/json"))
			{
				exceptionContext.Result = new JsonResult(errorViewModel);
			}
			exceptionContext.ExceptionHandled = true;
		}

		private ErrorModel BuildErrorModel(ExceptionContext exceptionContext)
		{
			var isShowErrors = _configuration.GetValue<bool>("IsShowErrors");
			var genericErrorMessage = _configuration.GetValue<string>("ErrorMessage");
			var isHttpContextNotNull = exceptionContext.HttpContext != null;
			var errorMessage = genericErrorMessage;

			var exceptionType = exceptionContext.Exception.GetType();
			if (exceptionType == typeof(BusinessValidationException))
			{
				errorMessage = exceptionContext.Exception.Message;
				_logger.Warn(exceptionContext.Exception.Message);

				if (isHttpContextNotNull)
					exceptionContext.HttpContext.Response.StatusCode = 412;
			}
			else if (exceptionType == typeof(BadRequestException))
			{
				errorMessage = exceptionContext.Exception.Message;
				_logger.Warn(exceptionContext.Exception.Message);

				if (isHttpContextNotNull)
					exceptionContext.HttpContext.Response.StatusCode = 400;
			}
			else
			{
				_logger.Error(exceptionContext.Exception);

				if (isShowErrors)
					errorMessage = exceptionContext.Exception.Message;

				if (isHttpContextNotNull)
					exceptionContext.HttpContext.Response.StatusCode = 500;
			}

			var errorModel = new ErrorModel
			{
				Message = errorMessage,
				Id = _correlationContextAccessor.CorrelationContext.CorrelationId
			};

			return errorModel;
		}
	}
}
