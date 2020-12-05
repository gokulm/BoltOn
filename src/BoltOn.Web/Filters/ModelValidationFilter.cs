using System.Linq;
using BoltOn.Web.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BoltOn.Web.Filters
{
    public class ModelValidationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            if (context.ModelState.IsValid)
                return;

            var errorMessage = string.Join(" | ", context.ModelState.Values.SelectMany(s => s.Errors)
				.Select(s => s.ErrorMessage));
            throw new BadRequestException(errorMessage);
        }
    }
}
