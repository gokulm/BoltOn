using BoltOn.Logging;
using BoltOn.Web.Exceptions;
using BoltOn.Web.Models;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;

namespace BoltOn.Web.Filters
{
	public class CustomExceptionFilter : ExceptionFilterAttribute
    {
        private readonly IConfiguration _configuration;
        private readonly ICorrelationContextAccessor _correlationContextAccessor;
        private readonly IBoltOnLogger<CustomExceptionFilter> _logger;

        public CustomExceptionFilter(IBoltOnLogger<CustomExceptionFilter> logger,
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
                return;

            var exception = exceptionContext.Exception;
            if (exception == null)
                return;

            var errorViewModel = BuildErrorViewModel(exceptionContext);
            if (exceptionContext.HttpContext != null &&
                exceptionContext.HttpContext.Request.ContentType != null &&
                exceptionContext.HttpContext.Request.ContentType.Contains("application/json"))
            {
                exceptionContext.Result = new JsonResult(errorViewModel);
            }
            else
            {
				var errorViewName = _configuration.GetValue<string>("ErrorViewName");
                var result = new ViewResult { ViewName = string.IsNullOrWhiteSpace(errorViewName) ? "Error" : errorViewName };
                var modelMetadata = new EmptyModelMetadataProvider();
                result.ViewData = new ViewDataDictionary<ErrorModel>(modelMetadata, exceptionContext.ModelState)
                {
                    Model = errorViewModel
                };
                exceptionContext.Result = result;
            }
            exceptionContext.ExceptionHandled = true;
        }

        private ErrorModel BuildErrorViewModel(ExceptionContext exceptionContext)
        {
            var isShowErrors = _configuration.GetValue<bool>("IsShowErrors");
            var genericErrorMessage = _configuration.GetValue<string>("ErrorMessage");
            var isHttpContextNotNull = exceptionContext.HttpContext != null;
            var errorMessage = genericErrorMessage;

            if (exceptionContext.Exception != null)
            {
                var exceptionType = exceptionContext.Exception.GetType();
                if (exceptionType == typeof(UserFriendlyException))
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
            }

            var errorViewModel = new ErrorModel
            {
                Message = errorMessage,
                Id = _correlationContextAccessor.CorrelationContext.CorrelationId
            };

            return errorViewModel;
        }
    }
}
