using ApiApplication.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace ApiApplication.Api.Utils
{
    public class ApiExceptionFilter : IActionFilter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var ex = context.Exception;
            ObjectResult result;
            if (ex == null)
            {
                return;
            }

            int statusCode = StatusCodes.Status500InternalServerError;
            var message = "Internal server error occured.";

            if (ex is BaseException baseException)
            {
                statusCode = baseException.StatusCode;
                message = baseException.Message;
            }

            var problemDetails = new ProblemDetails()
            {
                Status = statusCode,
                Detail = message
            };

            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status,
            };
            context.ExceptionHandled = true;

        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // no action needed
        }
    }
}
