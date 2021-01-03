using System.Linq;
using BoltOn.Logging;
using BoltOn.Web.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BoltOn.Web.Filters
{
    public class ModelValidationFilter : ActionFilterAttribute
    {
		private readonly IAppLogger<ModelValidationFilter> _logger;

		public ModelValidationFilter(IAppLogger<ModelValidationFilter> logger)
		{
			_logger = logger;
		}

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            if (context.ModelState.IsValid)
                return;

            _logger.Debug("Model is invalid");

            var errorMessage = string.Join(" | ", context.ModelState.Values.SelectMany(s => s.Errors)
				.Select(s => s.ErrorMessage));
            throw new BadRequestException(errorMessage);
        }
    }
}
